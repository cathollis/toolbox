import { defineStore } from 'pinia'
import { ref } from 'vue'

export const usePopupMessage = defineStore('systemMessage', () => {
  const message = ref<string>('')
  const level = ref<'error' | 'warning'>('error')
  const isShow = ref<boolean>(false)

  function showError(msg: string) {
    level.value = 'error'
    show(msg)
  }

  function show(msg: string) {
    isShow.value = true
  }

  function close() {
    isShow.value = false
    level.value = 'error'
    message.value = ''
  }

  return { message, level, isShow, showError }
})
