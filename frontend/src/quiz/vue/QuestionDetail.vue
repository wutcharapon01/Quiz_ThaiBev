<script setup>
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { getQuestionById, questionBank } from '../js/questions'

const props = defineProps({
  id: {
    type: [String, Number],
    required: true
  }
})

const selectedChoice = ref('')
const submitted = ref(false)

const currentQuestion = computed(() => getQuestionById(props.id))
const currentId = computed(() => Number(props.id))
const prevId = computed(() => (currentId.value > 1 ? currentId.value - 1 : null))
const nextId = computed(() => (currentId.value < questionBank.length ? currentId.value + 1 : null))

function submitAnswer() {
  submitted.value = true
}

function resetAnswer() {
  selectedChoice.value = ''
  submitted.value = false
}
</script>

<template>
  <main class="page-container" v-if="currentQuestion">
    <section class="question-panel">
      <p class="question-number">Question {{ currentQuestion.id }} / {{ questionBank.length }}</p>
      <h1>{{ currentQuestion.title }}</h1>
      <p class="prompt-text">{{ currentQuestion.prompt }}</p>

      <div class="choice-list">
        <label v-for="choice in currentQuestion.choices" :key="choice" class="choice-item">
          <input v-model="selectedChoice" type="radio" :value="choice" name="answer" />
          <span>{{ choice }}</span>
        </label>
      </div>

      <div class="action-row">
        <button class="primary-btn" type="button" :disabled="!selectedChoice" @click="submitAnswer">
          Submit
        </button>
        <button class="ghost-btn" type="button" @click="resetAnswer">Reset</button>
      </div>

      <p v-if="submitted" class="submit-note">Saved answer: {{ selectedChoice }}</p>

      <nav class="nav-row">
        <RouterLink v-if="prevId" :to="`/quiz/${prevId}`" class="ghost-btn">Previous</RouterLink>
        <RouterLink to="/" class="ghost-btn">Back Home</RouterLink>
        <RouterLink v-if="nextId" :to="`/quiz/${nextId}`" class="primary-btn">Next</RouterLink>
      </nav>
    </section>
  </main>

  <main class="page-container" v-else>
    <section class="question-panel">
      <h1>Question not found</h1>
      <RouterLink to="/" class="primary-btn">Back Home</RouterLink>
    </section>
  </main>
</template>