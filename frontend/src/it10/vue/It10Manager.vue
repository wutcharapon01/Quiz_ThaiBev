<script setup>
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { fetchIt10Questions, submitIt10Exam } from '../js/it10Api'
import '../css/it10.css'

const loading = ref(false)
const busy = ref(false)
const error = ref('')
const success = ref('')

const fullName = ref('')
const questions = ref([])
const answers = ref([])
const result = ref(null)

const isResultMode = computed(() => result.value !== null)

async function loadQuestions() {
  loading.value = true
  error.value = ''

  try {
    const payload = await fetchIt10Questions()
    questions.value = payload.questions || []
    answers.value = Array(questions.value.length).fill(null)
  } catch (err) {
    error.value = err.message
  } finally {
    loading.value = false
  }
}

function onPick(questionIndex, choiceIndex) {
  answers.value[questionIndex] = choiceIndex
}

async function submitExam() {
  error.value = ''
  success.value = ''

  if (!fullName.value.trim()) {
    error.value = 'กรุณากรอกชื่อ-นามสกุล'
    return
  }

  if (answers.value.some((x) => x === null || x === undefined)) {
    error.value = 'กรุณาเลือกคำตอบให้ครบทุกข้อ'
    return
  }

  busy.value = true
  try {
    const payload = await submitIt10Exam({
      fullName: fullName.value.trim(),
      answers: answers.value
    })

    result.value = payload
    success.value = 'บันทึกผลสอบเรียบร้อย'
  } catch (err) {
    error.value = err.message
  } finally {
    busy.value = false
  }
}

function resetExam() {
  result.value = null
  fullName.value = ''
  answers.value = Array(questions.value.length).fill(null)
  success.value = ''
  error.value = ''
}

onMounted(loadQuestions)
</script>

<template>
  <main class="it10-page">
    <header class="it10-head">IT 10-1</header>

    <section class="it10-body" v-if="!isResultMode">
      <div v-if="loading">กำลังโหลดข้อสอบ...</div>

      <template v-else>
        <div class="it10-name-row">
          <label for="full-name">ชื่อ - นามสกุล</label>
          <input id="full-name" v-model="fullName" type="text" maxlength="160" placeholder="กรอกชื่อผู้สอบ" />
        </div>

        <article v-for="(question, qIndex) in questions" :key="question.id" class="it10-card">
          <p class="it10-question">{{ qIndex + 1 }}. {{ question.text }}</p>
          <div class="it10-choices">
            <label v-for="(choice, cIndex) in question.choices" :key="`${question.id}-${cIndex}`" class="it10-choice">
              <input
                :name="`it10-question-${question.id}`"
                type="radio"
                :value="cIndex"
                :checked="answers[qIndex] === cIndex"
                @change="onPick(qIndex, cIndex)"
              />
              <span>{{ choice }}</span>
            </label>
          </div>
        </article>

        <div class="it10-actions">
          <button class="btn10 submit" type="button" :disabled="busy || loading" @click="submitExam">ส่งข้อสอบ</button>
          <RouterLink class="btn10 home" to="/">กลับหน้าหลัก</RouterLink>
        </div>

        <p v-if="error" class="it10-feedback error">{{ error }}</p>
        <p v-if="success" class="it10-feedback success">{{ success }}</p>
      </template>
    </section>

    <section class="it10-body" v-else>
      <article class="it10-result">
        <h3>IT 10-2</h3>
        <p>คุณ {{ result.fullName }} สอบได้คะแนน</p>
        <p class="it10-score">{{ result.score }} / {{ result.totalQuestions }}</p>
        <p>บันทึกเมื่อ: {{ new Date(result.createdAtUtc).toLocaleString('th-TH') }}</p>

        <div class="it10-actions">
          <button class="btn10 retry" type="button" @click="resetExam">สอบอีกครั้ง</button>
          <RouterLink class="btn10 home" to="/">กลับหน้าหลัก</RouterLink>
        </div>
      </article>
    </section>
  </main>
</template>
