<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { FFmpeg } from '@ffmpeg/ffmpeg'
import { fetchFile } from '@ffmpeg/util'

const ffmpeg = new FFmpeg()

const isProgressing = ref<boolean>(false)

const fileModel = ref<File | File[] | null>(null)
const file = computed(() => {
  if (Array.isArray(fileModel.value)) {
    if (fileModel.value.length <= 0) return null

    return fileModel.value[0]
  } else {
    return fileModel.value ? fileModel.value : null
  }
})

const fileInfo = reactive<{
  name?: string
  size?: number
}>({})
const infoList = ref<
  {
    type: string
    data: Record<string, string>
    raw: string
  }[]
>()

const onFileChange = async () => {
  const uploadedFile = file.value
  if (!uploadedFile) return

  fileInfo.name = uploadedFile.name
  fileInfo.size = Math.round(uploadedFile.size / 1024)

  try {
    isProgressing.value = true

    if (!ffmpeg.loaded) {
      await ffmpeg.load()
    }

    const inputFileExt = uploadedFile.name.substring(uploadedFile.name.lastIndexOf('.') + 1)
    const inputFileName = `media.${inputFileExt}`

    try {
      await ffmpeg.writeFile(inputFileName, await fetchFile(uploadedFile))

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
        const streamList = (outputInfo.streams as any[])
          .sort((x) => x['index'])
          .map((x) => {
            const infoDict = {} as Record<string, string>

            infoDict['编码'] = x['codec_long_name']
            const codecType = x['codec_type']
            infoDict['轨道类型'] = codecType

            if (codecType == 'audio') {
              infoDict['声道数'] = x['channels']
              infoDict['声道布局'] = x['channel_layout']
              infoDict['采样格式'] = x['sample_fmt']
              infoDict['采样率'] = x['sample_rate']
              infoDict['比特率（码率）'] = x['bit_rate']
            } else if (codecType == 'video') {
            }

            return {
              type: codecType,
              data: infoDict,
              raw: JSON.stringify(x),
            }
          })

        infoList.value = streamList
      }
    } finally {
      await ffmpeg.deleteFile(inputFileName)
      await ffmpeg.deleteFile('output.json')
    }
  } finally {
    isProgressing.value = false
  }
}
</script>

<template>
  <v-container class="d-flex flex-column ga-8">
    <v-card title="Operations">
      <div class="d-flex ga-4">
        <v-file-input
          accept="audio/*,video/*"
          clearable
          label="Select mult-media file."
          :loading="isProgressing"
          v-model="fileModel"
        ></v-file-input>

        <v-btn :disabled="!file" :loading="isProgressing" @click="onFileChange">Process</v-btn>
      </div>
    </v-card>

    <v-card title="File info">
      <div v-if="fileInfo">
        <p><strong>File name:</strong> {{ fileInfo.name }}</p>
        <p><strong>File size:</strong> {{ fileInfo.size }} KB</p>
      </div>
    </v-card>

    <v-card title="Media info">
      <div v-if="infoList">
        <div v-for="info in infoList">
          <h2>{{ info.type }}</h2>
          <v-list>
            <v-list-item v-for="(value, key) in info.data" :key="key" class="py-2">
              <v-row>
                <v-col cols="4" class="font-weight-bold text-grey-darken-1">{{ key }}</v-col>
                <v-col cols="8" class="text-truncate">{{ value }}</v-col>
              </v-row>
            </v-list-item>
          </v-list>
          <h3>原始信息</h3>
          <code>{{ info.raw }}</code>
        </div>
      </div>
    </v-card>
  </v-container>
</template>

<style scoped></style>
