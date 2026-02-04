<script setup>
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { fetchIt03Documents, resetIt03Documents, submitIt03Decision } from '../js/approvalApi'
import { STATUS, STATUS_LABEL } from '../js/documentData'
import '../css/approval-manager.css'

const documents = ref([])
const selectedIds = ref([])
const rowsToShow = ref(10)
const modalType = ref('')
const reasonText = ref('')
const errorText = ref('')
const busy = ref(false)
const loading = ref(false)

const displayOptions = [5, 10, 12, -1]

const visibleDocuments = computed(() => {
  if (rowsToShow.value === -1) {
    return documents.value
  }

  return documents.value.slice(0, rowsToShow.value)
})

const selectableRows = computed(() => visibleDocuments.value.filter((item) => item.status === STATUS.pending))

const isAllSelected = computed(() => {
  if (selectableRows.value.length === 0) {
    return false
  }

  return selectableRows.value.every((item) => selectedIds.value.includes(item.id))
})

function toggleSelectAll() {
  if (isAllSelected.value) {
    selectedIds.value = []
    return
  }

  selectedIds.value = selectableRows.value.map((item) => item.id)
}

function toggleSelected(id, checked) {
  if (checked) {
    if (!selectedIds.value.includes(id)) {
      selectedIds.value.push(id)
    }
    return
  }

  selectedIds.value = selectedIds.value.filter((itemId) => itemId !== id)
}

function openModal(type) {
  errorText.value = ''

  if (selectedIds.value.length === 0) {
    errorText.value = 'กรุณาเลือกรายการที่มีสถานะรออนุมัติก่อน'
    return
  }

  modalType.value = type
  reasonText.value = ''
}

function closeModal() {
  modalType.value = ''
  reasonText.value = ''
}

async function submitAction() {
  const reason = reasonText.value.trim()
  if (!reason) {
    errorText.value = 'กรุณากรอกเหตุผลก่อนบันทึก'
    return
  }

  busy.value = true
  errorText.value = ''

  try {
    await submitIt03Decision({
      ids: selectedIds.value,
      action: modalType.value,
      reason
    })

    selectedIds.value = []
    closeModal()
    await loadDocuments()
  } catch (error) {
    errorText.value = error.message
  } finally {
    busy.value = false
  }
}

async function resetDocuments() {
  busy.value = true
  errorText.value = ''

  try {
    await resetIt03Documents()
    selectedIds.value = []
    closeModal()
    await loadDocuments()
  } catch (error) {
    errorText.value = error.message
  } finally {
    busy.value = false
  }
}

function getModalTitle() {
  return modalType.value === STATUS.approved ? 'บันทึกการอนุมัติ' : 'บันทึกการไม่อนุมัติ'
}

function getModalButtonText() {
  return modalType.value === STATUS.approved ? 'อนุมัติ' : 'ไม่อนุมัติ'
}

async function loadDocuments() {
  loading.value = true
  errorText.value = ''

  try {
    documents.value = await fetchIt03Documents()
  } catch (error) {
    errorText.value = error.message
  } finally {
    loading.value = false
  }
}

onMounted(loadDocuments)
</script>

<template>
  <main class="it03-page">
    <header class="it03-header">IT 03-1</header>

    <section class="it03-body">
      <div class="it03-toolbar">
        <div class="it03-actions">
          <button class="btn-it03 btn-approve" type="button" :disabled="busy || loading" @click="openModal(STATUS.approved)">อนุมัติ</button>
          <button class="btn-it03 btn-reject" type="button" :disabled="busy || loading" @click="openModal(STATUS.rejected)">ไม่อนุมัติ</button>
          <button class="btn-it03 btn-neutral" type="button" :disabled="busy || loading" @click="resetDocuments">รีเซ็ตค่า</button>
          <RouterLink class="btn-it03 btn-neutral" to="/">กลับหน้าหลัก</RouterLink>
        </div>

        <label class="it03-filter" for="rowLimit">
          แสดงจำนวน:
          <select id="rowLimit" v-model.number="rowsToShow">
            <option v-for="option in displayOptions" :key="option" :value="option">
              {{ option === -1 ? 'ทั้งหมด' : option }}
            </option>
          </select>
        </label>
      </div>

      <div class="it03-table-wrap">
        <table class="it03-table">
          <thead>
            <tr>
              <th>
                <input type="checkbox" :checked="isAllSelected" :disabled="loading || busy" @change="toggleSelectAll" />
              </th>
              <th>รายการ</th>
              <th>เหตุผล</th>
              <th>สถานะเอกสาร</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="4">กำลังโหลดข้อมูล...</td>
            </tr>
            <tr v-else-if="visibleDocuments.length === 0">
              <td colspan="4">ไม่พบข้อมูล</td>
            </tr>
            <tr v-else v-for="doc in visibleDocuments" :key="doc.id" :class="{ 'row-disabled': doc.status !== STATUS.pending }">
              <td>
                <input
                  type="checkbox"
                  :checked="selectedIds.includes(doc.id)"
                  :disabled="doc.status !== STATUS.pending || busy"
                  @change="(event) => toggleSelected(doc.id, event.target.checked)"
                />
              </td>
              <td>{{ doc.title }}</td>
              <td class="reason-cell" :title="doc.reason || '-'">{{ doc.reason || '-' }}</td>
              <td>
                <span class="badge" :class="doc.status">{{ STATUS_LABEL[doc.status] }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <p v-if="errorText" class="feedback-text">{{ errorText }}</p>
    </section>
  </main>

  <section v-if="modalType" class="modal-backdrop">
    <article class="modal-box">
      <header class="modal-title">{{ getModalTitle() }}</header>
      <div class="modal-body">
        <label for="reason">เหตุผล :</label>
        <textarea id="reason" v-model="reasonText" />

        <div class="modal-actions">
          <button class="btn-it03" :class="modalType === STATUS.approved ? 'btn-approve' : 'btn-reject'" type="button" :disabled="busy" @click="submitAction">
            {{ busy ? 'กำลังบันทึก...' : getModalButtonText() }}
          </button>
          <button class="btn-it03 btn-reject" type="button" :disabled="busy" @click="closeModal">ยกเลิก</button>
        </div>
      </div>
    </article>
  </section>
</template>
