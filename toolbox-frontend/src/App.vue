<script setup lang="ts">
import { onMounted, ref, shallowRef } from 'vue'
import { menuList } from './router'
import { storeToRefs } from 'pinia'
import { useUser } from './stores/UserStore'
import { initializeMsal, msalInstance } from './utils/msal'
import microsoftApi from './apis/microsoftApi'
import { Uri } from 'monaco-editor'

const userStore = useUser()
const userStoreRef = storeToRefs(userStore)

const isNavOpen = shallowRef(true)

const avatarUrl = ref<string | null>(null)

const handleShowNav = () => {
  isNavOpen.value = true
}

const msalRequest = {
  scopes: ['User.Read'], // 用于读取个人资料（包括邮箱）
}

onMounted(async () => {
  try {
    await initializeMsal()
  } catch (err) {
    console.error('MSAL 初始化失败:', err)
  }

  if (userStoreRef.isLogin) {
    const url = await fetchPhoto()
    avatarUrl.value = url
  }
})

const handleLogin = async () => {
  try {
    // 使用弹窗方式登录
    await initializeMsal()
    const loginResponse = await msalInstance.loginPopup(msalRequest)
    console.log(loginResponse)

    userStoreRef.isLogin.value = true
    userStoreRef.token.value = loginResponse.accessToken
    userStoreRef.userName.value = loginResponse.account.username
    userStoreRef.name.value = loginResponse.account.name ?? ''

    const url = await fetchPhoto()
    avatarUrl.value = url
  } catch (error) {
    console.error('登录失败:', error)
  }
}

const handleLogout = () => {
  userStore.logout()
}

const handlePlatformLogout = () => {
  msalInstance.logoutPopup()
}

const fetchPhoto = async () => {
  const resp = await microsoftApi.get('me/photo/$value', { responseType: 'blob' })
  if (resp.status === 200) {
    console.log(resp)
    return URL.createObjectURL(resp.data)
  }
  return null
}
</script>

<template>
  <v-responsive class="border rounded">
    <v-app>
      <v-app-bar class="px-2" title="Hollis's toolbox">
        <template v-slot:prepend>
          <v-btn icon="mdi-menu" @click="handleShowNav"></v-btn>
        </template>

        <template v-slot:append>
          <v-btn
            v-if="userStoreRef.isLogin.value"
            size="large"
            class="text-none text-subtitle-1"
            variant="text"
          >
            <v-avatar v-if="avatarUrl === null" icon="mdi-account"></v-avatar>
            <v-avatar v-else :image="avatarUrl"></v-avatar>
            <span class="ml-2">{{ userStoreRef.name }}</span>

            <v-menu activator="parent">
              <v-list>
                <v-list-item>
                  <v-list-item-title>
                    <v-btn @click="handleLogout" variant="text"> Logout(this website) </v-btn>
                  </v-list-item-title>
                  <v-list-item-title>
                    <v-btn @click="handlePlatformLogout" variant="text"> Logout(everywhere) </v-btn>
                  </v-list-item-title>
                </v-list-item>
              </v-list>
            </v-menu>
          </v-btn>

          <v-btn
            v-else
            color="primary"
            elevation="4"
            variant="flat"
            prepend-icon="mdi-microsoft"
            block
            @click="handleLogin"
          >
            Login
          </v-btn>
        </template>
      </v-app-bar>

      <v-navigation-drawer v-model="isNavOpen">
        <v-list :key="index" v-for="(menuItem, index) in menuList">
          <v-list-item :to="menuItem.path" :title="menuItem.title"></v-list-item>
        </v-list>
      </v-navigation-drawer>

      <v-main>
        <router-view />
      </v-main>
    </v-app>
  </v-responsive>
</template>

<style scoped></style>
