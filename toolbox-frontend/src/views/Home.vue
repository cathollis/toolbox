<script setup lang="ts">
import { useUser } from '@/stores/UserStore'
import { storeToRefs } from 'pinia'

const userStore = useUser()
const userStoreRef = storeToRefs(userStore)
</script>

<template>
  <v-container>
    <v-card title="Home Page">
      <v-card-text>
        <div>
          Hello, {{ userStoreRef.isLogin.value ? userStoreRef.userName.value : 'someone' }} ,
          welcome to Hollis's toolbox.
        </div>
        <div v-if="!userStoreRef.isLogin.value">
          Login to use (non-existent) functions that need access api. And contact Hollis if you knew
          him/her to unlock more functions that need he/she pay money.
        </div>
      </v-card-text>
    </v-card>
  </v-container>
</template>
