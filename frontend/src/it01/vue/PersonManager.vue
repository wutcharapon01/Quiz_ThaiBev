<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { RouterLink } from 'vue-router'
import Datepicker from '@vuepic/vue-datepicker'
import '@vuepic/vue-datepicker/dist/main.css'
import { calculateAge, formatDate } from '../js/dateUtils'
import { createPerson, fetchPeople, fetchPersonById } from '../js/peopleApi'
import '../css/person-manager.css'

const people = ref([])
const loading = ref(false)
const saving = ref(false)
const errorMessage = ref('')
const addModalOpen = ref(false)
const viewModalOpen = ref(false)
const selectedPerson = ref(null)

const form = reactive({
  firstName: '',
  lastName: '',
  birthDate: null,
  remark: ''
})

const maxBirthDate = new Date()
const birthDateIso = computed(() => formatDateToIso(form.birthDate))
const computedAge = computed(() => calculateAge(birthDateIso.value))

async function loadPeople() {
  loading.value = true
  errorMessage.value = ''

  try {
    people.value = await fetchPeople()
  } catch (error) {
    errorMessage.value = error.message
  } finally {
    loading.value = false
  }
}

function openAddModal() {
  form.firstName = ''
  form.lastName = ''
  form.birthDate = null
  form.remark = ''
  errorMessage.value = ''
  addModalOpen.value = true
}

function closeAddModal() {
  addModalOpen.value = false
}

async function submitAdd() {
  if (!form.firstName.trim() || !form.lastName.trim() || !birthDateIso.value) {
    errorMessage.value = 'กรุณากรอกชื่อ นามสกุล และวันเกิดให้ครบ'
    return
  }

  saving.value = true
  errorMessage.value = ''

  try {
    await createPerson({
      firstName: form.firstName,
      lastName: form.lastName,
      birthDate: birthDateIso.value,
      remark: form.remark
    })
    addModalOpen.value = false
    await loadPeople()
  } catch (error) {
    errorMessage.value = error.message
  } finally {
    saving.value = false
  }
}

async function openViewModal(id) {
  errorMessage.value = ''

  try {
    selectedPerson.value = await fetchPersonById(id)
    viewModalOpen.value = true
  } catch (error) {
    errorMessage.value = error.message
  }
}

function closeViewModal() {
  viewModalOpen.value = false
  selectedPerson.value = null
}

onMounted(loadPeople)

function formatDateToIso(date) {
  if (!(date instanceof Date) || Number.isNaN(date.getTime())) {
    return ''
  }

  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}
</script>

<template>
  <main class="page-wrap">
    <header class="panel-header">IT 01-1</header>

    <section class="panel-body">
      <div class="toolbar">
        <RouterLink class="btn btn-neutral" to="/">กลับหน้าหลัก</RouterLink>
        <button class="btn btn-primary" type="button" @click="openAddModal">ADD</button>
      </div>

      <div class="table-wrap">
        <table class="people-table">
          <thead>
            <tr>
              <th>Id</th>
              <th>ชื่อ-นามสกุล</th>
              <th>วันเกิด</th>
              <th>อายุ</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="5">กำลังโหลดข้อมูล...</td>
            </tr>
            <tr v-else-if="people.length === 0">
              <td colspan="5">ยังไม่มีข้อมูล</td>
            </tr>
            <tr v-else v-for="person in people" :key="person.id">
              <td>{{ person.id }}</td>
              <td>{{ person.fullName }}</td>
              <td>{{ formatDate(person.birthDate) }}</td>
              <td>{{ person.age }}</td>
              <td>
                <button class="btn btn-link" type="button" @click="openViewModal(person.id)">View</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <p v-if="errorMessage" class="error-text">{{ errorMessage }}</p>
    </section>
  </main>

  <section v-if="addModalOpen" class="modal-backdrop" @click.self="closeAddModal">
    <article class="modal-box">
      <header class="modal-title">IT 01-2</header>
      <div class="modal-body">
        <div class="form-grid">
          <label for="firstName">ชื่อ - สกุล</label>
          <input id="firstName" v-model="form.firstName" type="text" placeholder="ชื่อ" />
          <input v-model="form.lastName" type="text" placeholder="นามสกุล" />

          <label for="birthDate">วันเกิด</label>
          <Datepicker
            id="birthDate"
            v-model="form.birthDate"
            class="span-2"
            :max-date="maxBirthDate"
            :enable-time-picker="false"
            teleport="body"
            :z-index="5000"
            auto-apply
            format="dd/MM/yyyy"
            placeholder="เลือกวันเกิด"
          />

          <label>อายุ</label>
          <input :value="computedAge" type="text" readonly class="span-2" />

          <label for="remark">อื่นๆ</label>
          <textarea id="remark" v-model="form.remark" class="span-2" />
        </div>

        <div class="modal-actions">
          <button class="btn btn-primary" type="button" :disabled="saving" @click="submitAdd">บันทึก</button>
          <button class="btn btn-danger" type="button" :disabled="saving" @click="closeAddModal">ยกเลิก</button>
        </div>
      </div>
    </article>
  </section>

  <section v-if="viewModalOpen && selectedPerson" class="modal-backdrop" @click.self="closeViewModal">
    <article class="modal-box">
      <header class="modal-title">IT 01-3</header>
      <div class="modal-body">
        <div class="form-grid">
          <label>ชื่อ - สกุล</label>
          <input :value="selectedPerson.firstName" type="text" readonly />
          <input :value="selectedPerson.lastName" type="text" readonly />

          <label>วันเกิด</label>
          <input :value="formatDate(selectedPerson.birthDate)" type="text" readonly class="span-2" />

          <label>อายุ</label>
          <input :value="selectedPerson.age" type="text" readonly class="span-2" />

          <label>อื่นๆ</label>
          <textarea :value="selectedPerson.remark || '-'" readonly class="span-2" />
        </div>

        <div class="modal-actions">
          <button class="btn btn-danger" type="button" @click="closeViewModal">ปิด</button>
        </div>
      </div>
    </article>
  </section>
</template>
