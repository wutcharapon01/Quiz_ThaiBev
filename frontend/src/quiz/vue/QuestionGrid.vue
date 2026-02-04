<script setup>
import { RouterLink } from 'vue-router'
import { questionBank } from '../js/questions'
</script>

<template>
  <main class="page-container">
    <section class="hero-block">
      <h1>Quiz Dashboard</h1>
      <p>Select a question to begin. This page supports all 10 questions.</p>
    </section>

    <section class="question-grid">
      <article v-for="question in questionBank" :key="question.id" class="question-card">
        <p class="question-number">Question {{ question.id }}</p>
        <h2>{{ question.title }}</h2>
        <p>{{ question.description }}</p>
        <RouterLink :to="`/quiz/${question.id}`" class="primary-btn">Start</RouterLink>
      </article>
    </section>
  </main>
</template>