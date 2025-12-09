<script setup lang="ts">
import { onMounted, shallowRef } from 'vue'
import { menuList } from './router'
import { storeToRefs } from 'pinia'
import { useUser } from './stores/UserStore'
import { initializeMsal, msalInstance } from './utils/msal'

const userStore = useUser()
const userStoreRef = storeToRefs(userStore)

const isNavOpen = shallowRef(true)

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
})

const handleLogin = async () => {
  try {
    // 使用弹窗方式登录
    await initializeMsal()
    const loginResponse = await msalInstance.loginPopup(msalRequest)
    const accessToken = loginResponse.accessToken

    userStoreRef.isLogin.value = true
    userStoreRef.token.value = loginResponse.accessToken
    userStoreRef.userName.value = loginResponse.account.username
  } catch (error) {
    console.error('登录失败:', error)
  }
}
</script>

<template>
  <v-responsive class="border rounded">
    <v-app>
      <v-app-bar title="Hollis's toolbox">
        <template v-slot:prepend>
          <v-btn @click="handleShowNav"> Show Menus </v-btn>
        </template>

        <template v-slot:append>
          <v-btn v-if="userStoreRef.isLogin.value" prepend-icon="mdi-account">
            {{ userStoreRef.userName }}
            <v-menu activator="parent">
              <v-list>
                <v-list-item>
                  <v-list-item-title>
                    <v-btn
                      @click="
                        () => {
                          userStoreRef.isLogin.value = false
                        }
                      "
                      variant="text"
                    >
                      Logout(this website)
                    </v-btn>
                  </v-list-item-title>
                  <v-list-item-title>
                    <v-btn
                      @click="
                        () => {
                          msalInstance.logoutPopup()
                        }
                      "
                      variant="text"
                    >
                      Logout(everywhere)
                    </v-btn>
                  </v-list-item-title>
                </v-list-item>
              </v-list>
            </v-menu>
          </v-btn>

          <v-btn v-else prepend-icon="mdi-microsoft" text="Login" @click="handleLogin"> </v-btn>
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
