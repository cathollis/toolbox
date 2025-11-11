<script setup lang="ts">
import { shallowRef } from 'vue'
import { menuList } from './router'

const isNavOpen = shallowRef(true)

const showNav = () => {
  isNavOpen.value = true
}
</script>

<template>
  <v-responsive class="border rounded">
    <v-app>
      <v-app-bar title="Toolbox">
        <template v-slot:prepend>
          <v-btn @click="showNav"> 显示导航栏 </v-btn>
        </template>
      </v-app-bar>

      <v-navigation-drawer v-model="isNavOpen">
        <v-list v-for="menuItem in menuList">
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
