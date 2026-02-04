import { createRouter, createWebHistory } from 'vue-router'
import HomeView from './views/HomeView.vue'
import QuizView from './views/QuizView.vue'

const routes = [
  {
    path: '/',
    name: 'home',
    component: HomeView
  },
  {
    path: '/quiz/:id',
    name: 'quiz',
    component: QuizView,
    props: true
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router