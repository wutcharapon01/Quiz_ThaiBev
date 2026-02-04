<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { RouterLink } from 'vue-router'
import Datepicker from '@vuepic/vue-datepicker'
import '@vuepic/vue-datepicker/dist/main.css'
import { createIt04Profile, getOccupations } from '../js/profileApi'
import '../css/profile-form.css'

const form = reactive({
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  birthDay: null,
  occupation: '',
  sex: 'Male',
  profileBase64: '',
  profileFileName: ''
})

const occupations = ref([])
const errors = reactive({
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  birthDay: '',
  occupation: '',
  sex: '',
  profileBase64: ''
})

const busy = ref(false)
const message = ref('')
const globalError = ref('')
const maxBirthDay = new Date()

const profilePreview = computed(() => form.profileBase64 || '')
const birthDayIso = computed(() => formatDateToIso(form.birthDay))

function resetErrors() {
  Object.keys(errors).forEach((key) => {
    errors[key] = ''
  })
  globalError.value = ''
}

function validateForm() {
  resetErrors()

  if (!form.firstName.trim()) {
    errors.firstName = 'กรุณากรอกชื่อ'
  }

  if (!form.lastName.trim()) {
    errors.lastName = 'กรุณากรอกนามสกุล'
  }

  if (!form.email.trim()) {
    errors.email = 'กรุณากรอกอีเมล'
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
    errors.email = 'รูปแบบอีเมลไม่ถูกต้อง'
  }

  if (!form.phone.trim()) {
    errors.phone = 'กรุณากรอกเบอร์โทร'
  } else if (!/^\d{9,15}$/.test(form.phone)) {
    errors.phone = 'เบอร์โทรต้องเป็นตัวเลข 9-15 หลัก'
  }

  if (!birthDayIso.value) {
    errors.birthDay = 'กรุณาเลือกวันเกิด'
  }

  if (!form.occupation) {
    errors.occupation = 'กรุณาเลือกอาชีพ'
  }

  if (!['Male', 'Female'].includes(form.sex)) {
    errors.sex = 'กรุณาเลือกเพศ'
  }

  if (!form.profileBase64) {
    errors.profileBase64 = 'กรุณาเลือกรูปโปรไฟล์'
  }

  return Object.values(errors).every((error) => !error)
}

async function onProfileChange(event) {
  const file = event.target.files?.[0]
  if (!file) {
    form.profileBase64 = ''
    form.profileFileName = ''
    return
  }

  if (!file.type.startsWith('image/')) {
    form.profileBase64 = ''
    form.profileFileName = ''
    event.target.value = ''
    errors.profileBase64 = 'ไฟล์ที่อัปโหลดต้องเป็นรูปภาพเท่านั้น'
    return
  }

  try {
    const base64 = await fileToBase64(file)
    await ensureImageCanLoad(base64)
    form.profileBase64 = base64
    form.profileFileName = file.name
    errors.profileBase64 = ''
  } catch {
    form.profileBase64 = ''
    form.profileFileName = ''
    event.target.value = ''
    errors.profileBase64 = 'ไฟล์รูปไม่ถูกต้องหรือไม่สามารถอ่านได้'
  }
}

async function submitForm() {
  message.value = ''

  if (!validateForm()) {
    return
  }

  busy.value = true

  try {
    const result = await createIt04Profile({
      firstName: form.firstName,
      lastName: form.lastName,
      email: form.email,
      phone: form.phone,
      profileBase64: form.profileBase64,
      occupation: form.occupation,
      sex: form.sex,
      birthDay: birthDayIso.value
    })

    message.value = result.message || `save data success id : ${result.id}`
    clearForm()
  } catch (error) {
    globalError.value = error.message
  } finally {
    busy.value = false
  }
}

function clearForm() {
  form.firstName = ''
  form.lastName = ''
  form.email = ''
  form.phone = ''
  form.birthDay = null
  form.occupation = ''
  form.sex = 'Male'
  form.profileBase64 = ''
  form.profileFileName = ''
  resetErrors()
}

async function loadOccupations() {
  try {
    occupations.value = await getOccupations()
  } catch {
    occupations.value = ['นักพัฒนาโปรแกรม', 'นักวิเคราะห์ข้อมูล', 'นักบัญชี']
  }
}

onMounted(loadOccupations)

function fileToBase64(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(String(reader.result || ''))
    reader.onerror = reject
    reader.readAsDataURL(file)
  })
}

function ensureImageCanLoad(dataUrl) {
  return new Promise((resolve, reject) => {
    const img = new Image()
    img.onload = () => resolve(true)
    img.onerror = reject
    img.src = dataUrl
  })
}

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
  <main class="it04-page">
    <header class="it04-header">IT 04-1</header>

    <section class="it04-body">
      <div class="it04-form">
        <div class="it04-field">
          <label for="firstName">First Name</label>
          <input id="firstName" v-model="form.firstName" type="text" />
          <span v-if="errors.firstName" class="it04-error">{{ errors.firstName }}</span>
        </div>

        <div class="it04-field">
          <label for="lastName">Last Name</label>
          <input id="lastName" v-model="form.lastName" type="text" />
          <span v-if="errors.lastName" class="it04-error">{{ errors.lastName }}</span>
        </div>

        <div class="it04-field">
          <label for="email">Email</label>
          <input id="email" v-model="form.email" type="email" />
          <span v-if="errors.email" class="it04-error">{{ errors.email }}</span>
        </div>

        <div class="it04-field">
          <label for="phone">Phone</label>
          <input id="phone" v-model="form.phone" type="text" inputmode="numeric" />
          <span v-if="errors.phone" class="it04-error">{{ errors.phone }}</span>
        </div>

        <div class="it04-field">
          <label for="profile">Profile</label>
          <div class="it04-file-row">
            <input id="profile" type="file" accept="image/*" @change="onProfileChange" />
          </div>
          <span v-if="errors.profileBase64" class="it04-error">{{ errors.profileBase64 }}</span>
          <img v-if="profilePreview" :src="profilePreview" alt="preview" class="it04-preview" />
        </div>

        <div class="it04-field">
          <label for="birthDay">Birth Day</label>
          <Datepicker
            id="birthDay"
            v-model="form.birthDay"
            :max-date="maxBirthDay"
            :enable-time-picker="false"
            teleport="body"
            :z-index="5000"
            auto-apply
            format="dd/MM/yyyy"
            placeholder="เลือกวันเกิด"
          />
          <span v-if="errors.birthDay" class="it04-error">{{ errors.birthDay }}</span>
        </div>

        <div class="it04-field">
          <label for="occupation">Occupation</label>
          <select id="occupation" v-model="form.occupation">
            <option value="">-- เลือกอาชีพ --</option>
            <option v-for="occupation in occupations" :key="occupation" :value="occupation">{{ occupation }}</option>
          </select>
          <span v-if="errors.occupation" class="it04-error">{{ errors.occupation }}</span>
        </div>

        <div class="it04-field">
          <label>Sex</label>
          <div class="it04-radio-group">
            <label><input v-model="form.sex" type="radio" value="Male" /> Male</label>
            <label><input v-model="form.sex" type="radio" value="Female" /> Female</label>
          </div>
          <span v-if="errors.sex" class="it04-error">{{ errors.sex }}</span>
        </div>
      </div>

      <p class="it04-hint">ระบบจะบันทึกรูปโปรไฟล์เป็นข้อมูล Base64</p>

      <div class="it04-actions">
        <button class="it04-btn save" type="button" :disabled="busy" @click="submitForm">Save</button>
        <button class="it04-btn clear" type="button" :disabled="busy" @click="clearForm">Clear</button>
        <RouterLink class="it04-btn back" to="/">กลับหน้าหลัก</RouterLink>
      </div>

      <p v-if="globalError" class="it04-error">{{ globalError }}</p>
      <p v-if="message" class="it04-success">{{ message }}</p>
    </section>
  </main>
</template>
