<script setup>
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { clearQueue, getCurrentQueue, issueQueue } from '../js/queueApi'
import '../css/queue-manager.css'

const step = ref('receive')
const queueNumber = ref('00')
const issuedAtText = ref('')
const busy = ref(false)
const error = ref('')

const headerText = computed(() => {
  if (step.value === 'receive') {
    return 'IT 05-1'
  }

  if (step.value === 'ticket') {
    return 'IT 05-2'
  }

  return 'IT 05-3'
})

async function loadCurrent() {
  try {
    const result = await getCurrentQueue()
    queueNumber.value = result.queueNumber || '00'
  } catch {
    queueNumber.value = '00'
  }
}

async function onReceiveTicket() {
  busy.value = true
  error.value = ''

  try {
    const result = await issueQueue()
    queueNumber.value = result.queueNumber || '00'
    issuedAtText.value = formatDateTime(result.issuedAtUtc)
    step.value = 'ticket'
  } catch (err) {
    error.value = err.message
  } finally {
    busy.value = false
  }
}

function onCreateQueue() {
  step.value = 'clear'
}

async function onClearQueue() {
  busy.value = true
  error.value = ''

  try {
    const result = await clearQueue()
    queueNumber.value = result.queueNumber || '00'
  } catch (err) {
    error.value = err.message
  } finally {
    busy.value = false
  }
}

function backToReceive() {
  step.value = 'receive'
  error.value = ''
}

function formatDateTime(value) {
  if (!value) {
    return '-'
  }

  const d = new Date(value)
  if (Number.isNaN(d.getTime())) {
    return '-'
  }

  return d.toLocaleString('th-TH')
}

onMounted(loadCurrent)
</script>

<template>
  <main class="it05-page">
    <header class="it05-header">{{ headerText }}</header>

    <section v-if="step === 'receive'" class="it05-body">
      <button class="ticket-btn" type="button" :disabled="busy" @click="onReceiveTicket">
        รับบัตรคิว
      </button>
      <br />
      <button class="secondary-btn" type="button" :disabled="busy" @click="onCreateQueue">สร้างคิว</button>
      <p class="muted">คิวปัจจุบัน: {{ queueNumber }}</p>
      <RouterLink class="link-back" to="/">กลับหน้าหลัก</RouterLink>
      <p v-if="error" class="error">{{ error }}</p>
    </section>

    <section v-else-if="step === 'ticket'" class="it05-body">
      <p>หมายเลขคิว</p>
      <p class="queue-no">{{ queueNumber }}</p>
      <p class="muted">วันที่ : {{ issuedAtText }}</p>
      <button class="action-btn" type="button" @click="onCreateQueue">กลับไปหน้าสร้างคิว</button>
      <p v-if="error" class="error">{{ error }}</p>
    </section>

    <section v-else class="it05-body">
      <button class="action-btn" type="button" :disabled="busy" @click="onClearQueue">ล้างคิว</button>
      <p style="margin-top: 0.9rem;">หมายเลขคิวปัจจุบัน</p>
      <p class="queue-no">{{ queueNumber }}</p>
      <button class="action-btn" type="button" @click="backToReceive">กลับไปรับคิวอีกครั้ง</button>
      <p v-if="error" class="error">{{ error }}</p>
    </section>
  </main>
</template>