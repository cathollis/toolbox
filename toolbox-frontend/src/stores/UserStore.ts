import { ref } from 'vue'
import { defineStore } from 'pinia'

export const useUser = defineStore(
  'toolbox.cathollis.com.user',
  () => {
    const isLogin = ref<boolean>(false)
    const userName = ref<string>('')
    const token = ref<string>('')

    return { isLogin, userName, token }
  },
  {
    persist: true,
  }
)
