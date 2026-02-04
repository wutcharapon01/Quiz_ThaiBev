<script setup>
import { onMounted, reactive, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { createIt08Question, deleteIt08Question, fetchIt08Questions } from '../js/it08Api'
import '../css/it08.css'

const questions = ref([])
const busy = ref(false)
const error = ref('')
const success = ref('')
const showAdd = ref(false)
const deleteTarget = ref(null)

const form = reactive({
  questionText: '',
  choice1: '',
  choice2: '',
  choice3: '',
  choice4: ''
})

async function loadQuestions() {
  busy.value = true
  error.value = ''
  try {
    questions.value = await fetchIt08Questions()
  } catch (err) {
    error.value = err.message
  } finally {
    busy.value = false
  }
}

function openAdd() {
  form.questionText = ''
  form.choice1 = ''
  form.choice2 = ''
  form.choice3 = ''
  form.choice4 = ''
  showAdd.value = true
}

function closeAdd() {
  showAdd.value = false
}

async function saveQuestion() {
  error.value = ''
  success.value = ''

  if (!form.questionText.trim() || !form.choice1.trim() || !form.choice2.trim() || !form.choice3.trim() || !form.choice4.trim()) {
    error.value = 'กรุณากรอกข้อมูลให้ครบทุกช่อง'
    return
  }

  busy.value = true
  try {
    await createIt08Question(form)
    showAdd.value = false
    success.value = 'บันทึกข้อสอบเรียบร้อย'
    await loadQuestions()
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
    await deleteIt08Question(deleteTarget.value.id)
    deleteTarget.value = null
    success.value = 'ลบข้อสอบเรียบร้อย และจัดลำดับใหม่แล้ว'
    await loadQuestions()
  } catch (err) {
    error.value = err.message
  } finally {
    busy.value = false
  }
}

onMounted(loadQuestions)
</script>

<template>
  <main class="it08-page">
    <header class="it08-head">IT 08-1</header>
    <section class="it08-body">
      <div class="it08-toolbar">
        <button class="btn08 add" type="button" @click="openAdd">เพิ่มข้อสอบ</button>
        <RouterLink class="btn08 back" to="/">กลับหน้าหลัก</RouterLink>
      </div>

      <div v-if="busy && questions.length === 0">กำลังโหลดข้อมูล...</div>
      <div v-else-if="questions.length === 0">ยังไม่มีข้อสอบ</div>
      <div v-else>
        <article v-for="item in questions" :key="item.id" class="q-item">
          <div class="q-title">{{ item.displayOrder }}. {{ item.questionText }} <button class="btn08 del" type="button" @click="requestDelete(item)">ลบ</button></div>
          <ol class="choice-list">
            <li v-for="choice in item.choices" :key="choice">{{ choice }}</li>
          </ol>
        </article>
      </div>

      <p v-if="error" class="feedback08 error">{{ error }}</p>
      <p v-if="success" class="feedback08 success">{{ success }}</p>
    </section>
  </main>

  <section v-if="showAdd" class="modal-backdrop">
    <article class="modal08">
      <h3>IT 08-2</h3>
      <div class="form08">
        <input v-model="form.questionText" type="text" placeholder="คำถาม" />
        <input v-model="form.choice1" type="text" placeholder="คำตอบ 1" />
        <input v-model="form.choice2" type="text" placeholder="คำตอบ 2" />
        <input v-model="form.choice3" type="text" placeholder="คำตอบ 3" />
        <input v-model="form.choice4" type="text" placeholder="คำตอบ 4" />
      </div>
      <div class="modal-actions08">
        <button class="btn08 save" type="button" :disabled="busy" @click="saveQuestion">บันทึก</button>
        <button class="btn08 del" type="button" :disabled="busy" @click="closeAdd">ยกเลิก</button>
      </div>
    </article>
  </section>

  <section v-if="deleteTarget" class="modal-backdrop">
    <article class="modal08">
      <p>ต้องการลบข้อสอบ ข้อที่ {{ deleteTarget.displayOrder }} หรือไม่ ?</p>
      <div class="modal-actions08">
        <button class="btn08 save" type="button" :disabled="busy" @click="confirmDelete">ตกลง</button>
        <button class="btn08 del" type="button" :disabled="busy" @click="cancelDelete">ยกเลิก</button>
      </div>
    </article>
  </section>
</template>