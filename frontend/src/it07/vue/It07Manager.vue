<script setup>
import { onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import QRCode from 'qrcode'
import { createIt07Code, deleteIt07Code, fetchIt07Codes } from '../js/it07Api'
import '../css/it07.css'

const inputCode = ref('')
const rows = ref([])
const busy = ref(false)
const error = ref('')
const success = ref('')
const qrTarget = ref(null)
const qrImage = ref('')
const deleteTarget = ref(null)

function formatCode(raw) {
  const cleaned = raw.toUpperCase().replace(/[^A-Z0-9]/g, '').slice(0, 30)
  return cleaned.replace(/(.{5})/g, '$1-').replace(/-$/, '')
}

function onInput(value) {
  inputCode.value = formatCode(value)
}

async function loadRows() {
  busy.value = true
  error.value = ''
  try {
    rows.value = await fetchIt07Codes()
  } catch (err) {
    error.value = err.message
  } finally {
    busy.value = false
  }
}

async function onAdd() {
  error.value = ''
  success.value = ''

  if (inputCode.value.length !== 35) {
    error.value = 'กรุณากรอกรหัส 30 หลัก รูปแบบ XXXXX-XXXXX-XXXXX-XXXXX-XXXXX-XXXXX'
    return
  }

  busy.value = true
  try {
    await createIt07Code(inputCode.value)
    inputCode.value = ''
    success.value = 'เพิ่มข้อมูลเรียบร้อย'
    await loadRows()
  } catch (err) {
    error.value = err.message
  } finally {
    busy.value = false
  }
}

async function openQr(item) {
  error.value = ''
  qrTarget.value = item

  try {
    qrImage.value = await QRCode.toDataURL(item.productCode, {
      errorCorrectionLevel: 'M',
      margin: 1,
      width: 280
    })
  } catch {
    qrImage.value = ''
    error.value = 'ไม่สามารถสร้าง QR Code ได้'
  }
}

function closeQr() {
  qrTarget.value = null
  qrImage.value = ''
}

function requestDelete(item) {
  deleteTarget.value = item
}

function cancelDelete() {
  deleteTarget.value = null
}

async function confirmDelete() {
  if (!deleteTarget.value) {
    return
  }

  busy.value = true
  error.value = ''
  success.value = ''

  try {
    await deleteIt07Code(deleteTarget.value.id)
    success.value = 'ลบข้อมูลสำเร็จ'
    deleteTarget.value = null
    await loadRows()
  } catch (err) {
    error.value = err.message
  } finally {
    busy.value = false
  }
}

onMounted(loadRows)
</script>

<template>
  <main class="it07-page">
    <header class="it07-head">IT 07-1</header>

    <section class="it07-body">
      <div class="it07-toolbar">
        <input
          :value="inputCode"
          class="it07-input"
          maxlength="35"
          placeholder="XXXXX-XXXXX-XXXXX-XXXXX-XXXXX-XXXXX"
          @input="(event) => onInput(event.target.value)"
        />
        <button class="btn-07 add" type="button" :disabled="busy" @click="onAdd">ADD</button>
        <RouterLink class="btn-07 back" to="/">กลับหน้าหลัก</RouterLink>
      </div>

      <div class="it07-table-wrap">
        <table class="it07-table">
          <thead>
            <tr>
              <th>Id</th>
              <th>รหัสสินค้า (30 หลัก)</th>
              <th>View</th>
              <th>Delete</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="busy && rows.length === 0">
              <td colspan="4">กำลังโหลดข้อมูล...</td>
            </tr>
            <tr v-else-if="rows.length === 0">
              <td colspan="4">ยังไม่มีข้อมูล</td>
            </tr>
            <tr v-else v-for="item in rows" :key="item.id">
              <td>{{ item.id }}</td>
              <td>{{ item.productCode }}</td>
              <td><button class="btn-07 qr" type="button" @click="openQr(item)">QR</button></td>
              <td><button class="btn-07 del" type="button" @click="requestDelete(item)">ลบ</button></td>
            </tr>
          </tbody>
        </table>
      </div>

      <p v-if="error" class="feedback-07 error">{{ error }}</p>
      <p v-if="success" class="feedback-07 success">{{ success }}</p>
    </section>
  </main>

  <section v-if="qrTarget" class="modal-backdrop">
    <article class="modal-07">
      <img v-if="qrImage" :src="qrImage" alt="qr" class="qr-image" />
      <p>{{ qrTarget.productCode }}</p>
      <div class="modal-actions-07">
        <button class="btn-07 add" type="button" @click="closeQr">ปิด</button>
      </div>
    </article>
  </section>

  <section v-if="deleteTarget" class="modal-backdrop">
    <article class="modal-07">
      <p>ต้องการลบข้อมูล รหัสสินค้า {{ deleteTarget.productCode }} หรือไม่ ?</p>
      <div class="modal-actions-07">
        <button class="btn-07 add" type="button" :disabled="busy" @click="confirmDelete">ตกลง</button>
        <button class="btn-07 del" type="button" :disabled="busy" @click="cancelDelete">ยกเลิก</button>
      </div>
    </article>
  </section>
</template>