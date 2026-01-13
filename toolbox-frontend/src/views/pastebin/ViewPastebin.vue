<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { CodeEditor } from 'monaco-editor-vue3'
import { useRoute } from 'vue-router'
import axiosInstance from '@/utils/request'

const isLoading = ref<boolean>(false)

const route = useRoute()
const language = ref<string>('plaintext')
const text = ref<string>('')

const code = route.params.code

onMounted(async () => {
  isLoading.value = true
  const pastebinItem = await axiosInstance.get('pastebinitem/' + code)
  text.value = pastebinItem.data
  isLoading.value = false
})
</script>

<template>
  <v-container class="d-flex flex-column ga-8">
    <v-card title="Operation">
      <v-card-text>
        <v-select v-model="language" label="Language" :items="['plaintext', 'text']"></v-select>
      </v-card-text>
    </v-card>

    <v-card title="Content">
      <v-card-text>
        <div v-if="isLoading">Loading</div>
        <div v-else style="height: 20rem">
          <CodeEditor
            v-model:value="text"
            :language="language"
            theme="vs-dark"
            :options="{ readOnly: true }"
          />
        </div>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<style scoped></style>
