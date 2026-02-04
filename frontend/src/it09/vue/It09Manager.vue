<script setup>
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { createIt09Comment, fetchIt09Thread } from '../js/it09Api'
import '../css/it09.css'

const post = ref(null)
const comments = ref([])
const messageInput = ref('')
const error = ref('')
const busy = ref(false)
const loading = ref(false)

const sortedComments = computed(() => comments.value)

function initial(name) {
  if (!name) {
    return 'B'
  }
  return name[0].toUpperCase()
}

function formatDateTime(utcText) {
  if (!utcText) {
    return '-'
  }

  const d = new Date(utcText)
  if (Number.isNaN(d.getTime())) {
    return '-'
  }

  return d.toLocaleString('th-TH')
}

async function loadThread() {
  loading.value = true
  error.value = ''

  try {
    const data = await fetchIt09Thread()
    post.value = data.post
    comments.value = data.comments || []
  } catch (err) {
    error.value = err.message
  } finally {
    loading.value = false
  }
}

async function submitComment() {
  const text = messageInput.value.trim()
  if (!text) {
    return
  }

  busy.value = true
  error.value = ''

  try {
    const added = await createIt09Comment(text)
    comments.value.push(added)
    messageInput.value = ''
  } catch (err) {
    error.value = err.message
  } finally {
    busy.value = false
  }
}

function onInputKeydown(event) {
  if (event.key === 'Enter' && !event.shiftKey) {
    event.preventDefault()
    if (!busy.value) {
      submitComment()
    }
  }
}

onMounted(loadThread)
</script>

<template>
  <main class="it09-page">
    <header class="it09-head">IT 09-1</header>
    <section class="it09-body" v-if="post">
      <div class="row09">
        <div class="avatar09">{{ initial(post.author) }}</div>
        <div>
          <strong>{{ post.author }}</strong>
          <div class="post-meta09">{{ formatDateTime(post.createdAtUtc) }}</div>
        </div>
      </div>

      <img :src="post.imageUrl" class="post-image09" alt="post" />

      <div class="row09" style="margin-top: 0.9rem; margin-bottom: 0.2rem;">
        <div class="avatar09">B</div>
        <div><strong>Blend 285</strong></div>
      </div>

      <input
        v-model="messageInput"
        class="comment-input09"
        type="text"
        placeholder="Comment"
        maxlength="300"
        :disabled="busy"
        @keydown="onInputKeydown"
      />
      <div class="helper09">พิมพ์ข้อความแล้วกด Enter เพื่อส่ง</div>

      <div class="comment09" v-for="item in sortedComments" :key="item.id" style="margin-top: 0.75rem;">
        <div class="row09" style="margin-bottom: 0.2rem; margin-left: -44px;">
          <div class="avatar09">{{ initial(item.commenter) }}</div>
          <div><strong>{{ item.commenter }}</strong></div>
        </div>
        <div>{{ item.message }}</div>
      </div>

      <p v-if="error" class="error09">{{ error }}</p>
      <RouterLink class="link09" to="/">กลับหน้าหลัก</RouterLink>
    </section>
    <section class="it09-body" v-else-if="loading">
      <p>กำลังโหลดข้อมูล...</p>
    </section>
    <section class="it09-body" v-else>
      <p class="error09">{{ error || 'ไม่สามารถโหลดข้อมูลได้' }}</p>
      <button class="retry09" type="button" @click="loadThread">ลองใหม่</button>
      <RouterLink class="link09" to="/">กลับหน้าหลัก</RouterLink>
    </section>
  </main>
</template>
