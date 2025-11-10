<script setup lang="ts">
import { FFmpeg } from '@ffmpeg/ffmpeg'
import { fetchFile } from '@ffmpeg/util'
import { reactive, ref } from 'vue'

const ffmpeg = new FFmpeg()

const fileInfo = reactive<{
  name?: string
  size?: number
  codec?: string
  format?: string
  sampleRate?: number
  bitRate?: number
}>({})
const infoList = ref<string[]>()

const onFileChange = async (e: Event) => {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return

  fileInfo.name = file.name
  fileInfo.size = Math.round(file.size / 1024)

  if (!ffmpeg.loaded) {
    await ffmpeg.load()
  }

  const inputFileExt = file.name.substring(file.name.lastIndexOf('.') + 1)
  const inputFileName = `media.${inputFileExt}`

  await ffmpeg.writeFile(inputFileName, await fetchFile(file))

  await ffmpeg.ffprobe([
    '-v',
    'quiet',
    '-print_format',
    'json',
    '-show_format',
    '-show_streams',
    inputFileName,
    '-o',
    'output.json',
  ])

  const outputFile = await ffmpeg.readFile('output.json')
  if (outputFile) {
    const outputFileText =
      typeof outputFile === 'string' ? outputFile : new TextDecoder().decode(outputFile)

    const outputInfo = JSON.parse(outputFileText)
    infoList.value = outputInfo.streams as string[]
  }

  await ffmpeg.deleteFile(inputFileName)
  await ffmpeg.deleteFile('output.json')
}
</script>

<template>
  <h1>Audio info</h1>
  <input type="file" @change="onFileChange" accept="audio/*" />
  <div v-if="fileInfo">
    <p><strong>文件名：</strong> {{ fileInfo.name }}</p>
    <p><strong>文件大小：</strong> {{ fileInfo.size }} KB</p>
    <div v-if="infoList">
      <pre v-for="value in infoList">{{ value }}</pre>
    </div>
  </div>
</template>

<style scoped></style>
