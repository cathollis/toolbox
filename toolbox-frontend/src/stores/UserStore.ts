import { ref } from 'vue'
import { defineStore } from 'pinia'
import type { AuthenticationResult } from '@azure/msal-browser'

export const useUser = defineStore(
  'toolbox.cathollis.com.user',
  () => {
    const isLogin = ref<boolean>(false)
    const authInfo = ref<AuthenticationResult | null>(null)

    const logout = () => {
      isLogin.value = false
      authInfo.value = null
    }

    return { logout, isLogin }
  },
  {
    persist: true,
  },
)
