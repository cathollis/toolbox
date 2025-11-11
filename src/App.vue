<script setup lang="ts">
import { menuList } from './router'
</script>

<template>
  <v-responsive class="border rounded">
    <v-app>
      <v-app-bar title="Toolbox"></v-app-bar>

      <v-navigation-drawer>
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
