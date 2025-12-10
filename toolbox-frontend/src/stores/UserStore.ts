import { ref } from 'vue'
import { defineStore } from 'pinia'

export const useUser = defineStore(
  'toolbox.cathollis.com.user',
  () => {
    const isLogin = ref<boolean>(false)
    const userName = ref<string>('')
    const name = ref<string>('')
    const token = ref<string | null>(null)

    const logout = () => {
      isLogin.value = false
      userName.value = ''
      name.value = ''
      token.value = ''
    }

    return { logout, isLogin, userName, name, token }
  },
  {
    persist: true,
  },
)
