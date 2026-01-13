<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { jwtDecode } from 'jwt-decode'

const tokenModel = ref<string>('')
const result = ref<string>('')

const handleDecodeClick = async () => {
  let token = tokenModel.value.trim()
  if (token.length > 0) {
    const bearerConst = 'BEARER'
    if (token.toUpperCase().startsWith(bearerConst)) {
      token = token.substring(0, bearerConst.length).trim()
    }

    try {
      result.value = JSON.stringify(jwtDecode(tokenModel.value))
    } catch (ex) {
      alert(ex)
    }
  }
}

onMounted(() => {
  // use code to send HTTP request
})
</script>

<template>
  <v-container class="d-flex flex-column ga-8">
    <v-card title="Content">
      <v-card-text>
        <div class="d-flex ga-4 flex-row flex-xs-column">
          <div class="flex-1-0" style="height: 10rem">
            <v-textarea v-model="tokenModel" label="JWT" no-resize clearable></v-textarea>
          </div>
          <div class="flex-1-0" style="height: 10rem">
            <v-textarea v-model="result" label="JWT Info" no-resize readonly></v-textarea>
          </div>
        </div>
      </v-card-text>
      <v-card-actions>
        <v-btn @click="handleDecodeClick">DECODE</v-btn>
      </v-card-actions>
    </v-card>
  </v-container>
</template>

<style scoped></style>
