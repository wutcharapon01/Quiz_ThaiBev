<script setup>
import { onMounted, ref, watch } from 'vue'
import JsBarcode from 'jsbarcode'

const props = defineProps({
  value: {
    type: String,
    required: true
  }
})

const svgRef = ref(null)

function drawBarcode() {
  if (!svgRef.value) {
    return
  }

  JsBarcode(svgRef.value, props.value, {
    format: 'CODE39',
    displayValue: false,
    margin: 0,
    height: 34,
    width: 1.4,
    background: 'transparent'
  })
}

onMounted(drawBarcode)
watch(() => props.value, drawBarcode)
</script>

<template>
  <svg ref="svgRef" class="barcode-svg"></svg>
</template>