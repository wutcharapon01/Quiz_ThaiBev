<script setup>
import { computed } from 'vue'
import PersonManager from '../it01/vue/PersonManager.vue'
import AuthManager from '../it02/vue/AuthManager.vue'
import ApprovalManager from '../it03/vue/ApprovalManager.vue'
import ProfileFormManager from '../it04/vue/ProfileFormManager.vue'
import QueueManager from '../it05/vue/QueueManager.vue'
import BarcodeManager from '../it06/vue/BarcodeManager.vue'
import It07Manager from '../it07/vue/It07Manager.vue'
import It08Manager from '../it08/vue/It08Manager.vue'
import It09Manager from '../it09/vue/It09Manager.vue'
import It10Manager from '../it10/vue/It10Manager.vue'
import QuestionDetail from '../quiz/vue/QuestionDetail.vue'

const props = defineProps({
  id: {
    type: [String, Number],
    required: true
  }
})

const questionId = computed(() => Number(props.id))
const isQuestionOne = computed(() => questionId.value === 1)
const isQuestionTwo = computed(() => questionId.value === 2)
const isQuestionThree = computed(() => questionId.value === 3)
const isQuestionFour = computed(() => questionId.value === 4)
const isQuestionFive = computed(() => questionId.value === 5)
const isQuestionSix = computed(() => questionId.value === 6)
const isQuestionSeven = computed(() => questionId.value === 7)
const isQuestionEight = computed(() => questionId.value === 8)
const isQuestionNine = computed(() => questionId.value === 9)
const isQuestionTen = computed(() => questionId.value === 10)
</script>

<template>
  <PersonManager v-if="isQuestionOne" />
  <AuthManager v-else-if="isQuestionTwo" />
  <ApprovalManager v-else-if="isQuestionThree" />
  <ProfileFormManager v-else-if="isQuestionFour" />
  <QueueManager v-else-if="isQuestionFive" />
  <BarcodeManager v-else-if="isQuestionSix" />
  <It07Manager v-else-if="isQuestionSeven" />
  <It08Manager v-else-if="isQuestionEight" />
  <It09Manager v-else-if="isQuestionNine" />
  <It10Manager v-else-if="isQuestionTen" />
  <QuestionDetail v-else :id="props.id" />
</template>
