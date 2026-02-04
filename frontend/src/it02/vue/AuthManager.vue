<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { fetchCurrentUser, loginUser, registerUser } from '../js/authApi'
import { clearAuthSession, getAuthToken, getSavedUsername, saveAuthSession } from '../js/authStorage'
import '../css/auth-manager.css'

const loginForm = reactive({
  username: getSavedUsername(),
  password: ''
})

const registerForm = reactive({
  username: '',
  password: '',
  confirmPassword: ''
})

const showRegisterModal = ref(false)
const loginError = ref('')
const registerError = ref('')
const successMessage = ref('')
const busy = ref(false)
const currentUser = ref(null)

const isAuthenticated = computed(() => !!currentUser.value)

async function validateToken() {
  const token = getAuthToken()
  if (!token) {
    return
  }

  try {
    currentUser.value = await fetchCurrentUser()
  } catch {
    clearAuthSession()
    currentUser.value = null
  }
}

function openRegister() {
  registerForm.username = ''
  registerForm.password = ''
  registerForm.confirmPassword = ''
  registerError.value = ''
  successMessage.value = ''
  showRegisterModal.value = true
}

function closeRegister() {
  showRegisterModal.value = false
}

async function submitRegister() {
  registerError.value = ''
  successMessage.value = ''

  if (!registerForm.username.trim() || !registerForm.password || !registerForm.confirmPassword) {
    registerError.value = 'กรุณากรอกข้อมูลให้ครบ'
    return
  }

  busy.value = true

  try {
    await registerUser({
      username: registerForm.username,
      password: registerForm.password,
      confirmPassword: registerForm.confirmPassword
    })

    loginForm.username = registerForm.username
    loginForm.password = registerForm.password
    showRegisterModal.value = false
    successMessage.value = 'สมัครสมาชิกสำเร็จ กรุณาเข้าสู่ระบบ'
  } catch (error) {
    registerError.value = error.message
  } finally {
    busy.value = false
  }
}

async function submitLogin() {
  loginError.value = ''
  successMessage.value = ''

  if (!loginForm.username.trim() || !loginForm.password) {
    loginError.value = 'กรุณากรอก User และ Password'
    return
  }

  busy.value = true

  try {
    const result = await loginUser({
      username: loginForm.username,
      password: loginForm.password
    })

    saveAuthSession(result.token, result.username)
    currentUser.value = await fetchCurrentUser()
    loginForm.password = ''
  } catch (error) {
    loginError.value = error.message || 'เข้าสู่ระบบไม่สำเร็จ'
  } finally {
    busy.value = false
  }
}

function logout() {
  clearAuthSession()
  currentUser.value = null
  loginForm.password = ''
}

onMounted(validateToken)
</script>

<template>
  <main class="auth-page">
    <section class="auth-grid">
      <article class="auth-card">
        <header class="card-head">IT 02-1 Login</header>
        <div class="card-body">
          <div class="field">
            <label for="loginUser">User</label>
            <input id="loginUser" v-model="loginForm.username" type="text" autocomplete="username" />
          </div>

          <div class="field">
            <label for="loginPass">Password</label>
            <input id="loginPass" v-model="loginForm.password" type="password" autocomplete="current-password" />
          </div>

          <div class="actions">
            <button class="auth-btn" type="button" :disabled="busy" @click="submitLogin">ส่งเข้าสู่ระบบ</button>
            <button class="auth-link-btn" type="button" :disabled="busy" @click="openRegister">สมัครสมาชิก</button>
            <RouterLink class="auth-link-btn" to="/">กลับหน้าหลัก</RouterLink>
          </div>

          <p v-if="loginError" class="auth-error">{{ loginError }}</p>
          <p v-if="successMessage" class="success-text">{{ successMessage }}</p>
        </div>
      </article>

    </section>

    <article v-if="isAuthenticated" class="welcome-card">
      <header class="card-head">IT 02-3</header>
      <div class="card-body">
        <p>Welcome User : {{ currentUser.username }}</p>
        <div class="actions" style="margin-top: 0.8rem;">
          <button class="auth-btn" type="button" @click="logout">Logout</button>
        </div>
      </div>
    </article>
  </main>

  <section v-if="showRegisterModal" class="modal-backdrop">
    <article class="modal-box">
      <header class="card-head">IT 02-2 Register</header>
      <div class="card-body">
        <div class="field">
          <label for="regUser">User</label>
          <input id="regUser" v-model="registerForm.username" type="text" autocomplete="username" />
        </div>

        <div class="field">
          <label for="regPass">Password</label>
          <input id="regPass" v-model="registerForm.password" type="password" autocomplete="new-password" />
        </div>

        <div class="field">
          <label for="regConfirm">Confirm Password</label>
          <input id="regConfirm" v-model="registerForm.confirmPassword" type="password" autocomplete="new-password" />
        </div>

        <div class="actions">
          <button class="auth-btn" type="button" :disabled="busy" @click="submitRegister">สมัครสมาชิก</button>
          <button class="auth-link-btn" type="button" :disabled="busy" @click="closeRegister">ยกเลิก</button>
        </div>

        <p v-if="registerError" class="auth-error">{{ registerError }}</p>
      </div>
    </article>
  </section>
</template>
