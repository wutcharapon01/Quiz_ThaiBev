<script setup>
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import Code39Barcode from './Code39Barcode.vue'
import { createProductCode, deleteProductCode, fetchProductCodes } from '../js/productCodeApi'
import '../css/barcode-manager.css'

const codeInput = ref('')
const rows = ref([])
const busy = ref(false)
const error = ref('')
const success = ref('')
const deleteTarget = ref(null)

const normalizedCode = computed(() => formatCode(codeInput.value))

function formatCode(raw) {
  const cleaned = raw.replace(/\D/g, '').slice(0, 16)
  return cleaned.replace(/(.{4})/g, '$1-').replace(/-$/, '')
}

function onCodeInput(value) {
  codeInput.value = formatCode(value)
}

async function loadRows() {
  busy.value = true
  error.value = ''

  try {
    rows.value = await fetchProductCodes()
  } catch (err) {
    error.value = err.message
  } finally {
    busy.value = false
  }
}

async function onAdd() {
  error.value = ''
  success.value = ''

  if (normalizedCode.value.length !== 19) {
    error.value = 'กรุณากรอกรหัสให้ครบ 16 ตัวอักษรในรูปแบบ XXXX-XXXX-XXXX-XXXX'
    return
  }

  busy.value = true

  try {
    await createProductCode(normalizedCode.value)
    codeInput.value = ''
    success.value = 'เพิ่มรหัสสินค้าเรียบร้อย'
    await loadRows()
  } catch (err) {
    error.value = err.message
  } finally {
    busy.value = false
  }
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
    await deleteProductCode(deleteTarget.value.id)
    success.value = 'ลบรหัสสินค้าเรียบร้อย'
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
  <main class="it06-page">
    <header class="it06-header">IT 06-1</header>

    <section class="it06-body">
      <div class="it06-toolbar">
        <input
          :value="codeInput"
          class="it06-input"
          maxlength="19"
          inputmode="numeric"
          placeholder="XXXX-XXXX-XXXX-XXXX"
          @input="(event) => onCodeInput(event.target.value)"
        />
        <button class="it06-btn add" type="button" :disabled="busy" @click="onAdd">ADD</button>
        <RouterLink class="it06-btn back" to="/">กลับหน้าหลัก</RouterLink>
      </div>

      <div class="it06-table-wrap">
        <table class="it06-table">
          <thead>
            <tr>
              <th>Id</th>
              <th>รหัสสินค้า</th>
              <th>บาร์โค้ดสินค้า</th>
              <th>Action</th>
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
              <td><Code39Barcode :value="item.productCode" /></td>
              <td>
                <button class="delete-btn" type="button" :disabled="busy" @click="requestDelete(item)">ลบ</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <p v-if="error" class="feedback error">{{ error }}</p>
      <p v-if="success" class="feedback success">{{ success }}</p>
    </section>
  </main>

  <section v-if="deleteTarget" class="modal-backdrop">
    <article class="modal-box">
      <p>ต้องการลบรหัสสินค้า <strong>{{ deleteTarget.productCode }}</strong> หรือไม่ ?</p>
      <div class="modal-actions">
        <button class="it06-btn add" type="button" :disabled="busy" @click="confirmDelete">ตกลง</button>
        <button class="delete-btn" type="button" :disabled="busy" @click="cancelDelete">ยกเลิก</button>
      </div>
    </article>
  </section>
</template>
