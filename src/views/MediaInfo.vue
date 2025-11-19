<script setup lang="ts">
import { computed, ref } from 'vue'
import { CodeEditor } from 'monaco-editor-vue3'
import { FFmpeg } from '@ffmpeg/ffmpeg'
import { fetchFile } from '@ffmpeg/util'
import humanizeDuration from 'humanize-duration'
import { humanizeBinaryBytes } from '@/utils/humanize'

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

interface IFfmpegFormat {
  nb_streams: number
  nb_programs: number
  format_name: string
  format_long_name: string
  start_time: string
  duration: string
  size: string
  bit_rate: string
  tags: ReadonlyMap<string, string>
}

interface IFfmpegStream {
  index: number
  codec_type: string
  codec_tag_string: string
  id: string
  time_base: string
  start_pts: number
  start_time: string
  duration_ts: number
  duration: string
  bit_rate: string
  tags?: ReadonlyMap<string, string>
}

const fileInfoView = ref<{
  name?: string
  size?: number
}>({})

const rawInfo = ref<{
  format?: IFfmpegFormat
  streams?: IFfmpegStream[]
}>({})

const rawInfoView = computed<string | null>(() => {
  if (!rawInfo.value.format || !rawInfo.value.streams) return null

  return JSON.stringify(rawInfo.value, null, 2)
})

const bitRateToString = (inputBitRate: number) => {
  if (Number.isNaN(inputBitRate)) return '-'

  const kbpsValue = inputBitRate / 1000
  if (kbpsValue <= 1000) {
    return `${kbpsValue} kbps`
  }

  return `${kbpsValue / 1000} mbps`
}

interface IInfoItemView {
  title: string
  data: string | any
  tips: string
  hasProcessed: boolean
}

const formatInfoView = computed<IInfoItemView[]>(() => {
  const format = rawInfo.value.format
  if (!format) return []

  const infoList = Object.entries(format)
    .map(([key, val]) => {
      let infoItem = {
        title: key,
        data: val,
        tips: '',
        hasProcessed: false,
      }

      if (key === 'format_name') {
        infoItem.hasProcessed = true
        infoItem.title = 'Format Name'
      } else if (key === 'format_long_name') {
        infoItem.hasProcessed = true
        infoItem.title = 'Format Name(full)'
      } else if (key === 'duration') {
        infoItem.hasProcessed = true

        const duration = Number(format.duration)
        const durationMs = Number.isNaN(duration) ? -1 : duration * 1000

        infoItem.title = 'Duration'
        infoItem.data = humanizeDuration(durationMs)
      } else if (key === 'size') {
        infoItem.hasProcessed = true

        const size = Number(format.size)

        infoItem.title = 'Size'
        infoItem.data = humanizeBinaryBytes(size)
        infoItem.tips = '1024 Bytes'
      } else if (key === 'bit_rate') {
        infoItem.hasProcessed = true

        const rate = Number(format.bit_rate)
        infoItem.title = 'Bit Rate'
        infoItem.data = bitRateToString(rate)
        infoItem.tips = '1000 bits pre second'
      }

      return infoItem
    })
    .sort((a, b) => Number(b.hasProcessed) - Number(a.hasProcessed))

  return infoList
})

interface IMediaInfoViewItem {
  title: string
  infoList: IInfoItemView[]
}

const mediaInfoView = computed<IMediaInfoViewItem[]>(() => {
  const streamList = rawInfo.value.streams
  if (!streamList) return []

  const mediaInfoList = streamList.map((stream) => {
    const streamInfoList = Object.entries(stream)
      .map(([key, val]) => {
        let infoItem = {
          title: key,
          data: val,
          tips: '',
          hasProcessed: false,
        }

        return infoItem
      })
      .sort((a, b) => Number(b.hasProcessed) - Number(a.hasProcessed))

    return {
      title: `${stream.index}-${stream.codec_type}`,
      infoList: streamInfoList,
    }
  })

  return mediaInfoList
})

const onFileChange = async () => {
  const uploadedFile = file.value
  if (!uploadedFile) return

  fileInfoView.value.name = uploadedFile.name
  fileInfoView.value.size = Math.round(uploadedFile.size / 1024)

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
        if (outputInfo) {
          rawInfo.value = outputInfo
        }
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
      <v-card-text>
        <v-file-input
          accept="audio/*,video/*"
          clearable
          label="Select mult-media file."
          :loading="isProgressing"
          v-model="fileModel"
        ></v-file-input>
      </v-card-text>
      <v-card-actions>
        <v-btn :disabled="!file" :loading="isProgressing" @click="onFileChange">Process</v-btn>
      </v-card-actions>
    </v-card>

    <v-card title="File info">
      <v-card-text>
        <p><strong>File name:</strong> {{ fileInfoView.name ?? '-' }}</p>
        <p><strong>File size:</strong> {{ fileInfoView.size ?? '-' }} KB</p>
      </v-card-text>
    </v-card>

    <v-card title="Format Info">
      <v-list>
        <v-list-item v-for="info in formatInfoView">
          <v-row>
            <v-col cols="4" class="font-weight-bold text-grey-darken-1">
              <v-tooltip v-if="info.tips" :text="info.tips">
                <template v-slot:activator="">
                  <p>{{ info.title }}</p>
                </template>
              </v-tooltip>
              <p v-else>{{ info.title }}</p>
            </v-col>
            <v-col cols="8" class="text-truncate">
              <p v-if="typeof info.data === 'string' || typeof info.data === 'number'">
                {{ info.data.toString() }}
              </p>
              <div v-else>
                <p v-for="[infoKey, infoVal] in Object.entries(info.data)">
                  {{ infoKey }} : {{ infoVal }}
                </p>
              </div>
            </v-col>
          </v-row>
        </v-list-item>
      </v-list>
    </v-card>

    <v-card title="Media info">
      <v-card-text>
        <v-expansion-panels>
          <v-expansion-panel v-for="mediaInfoItem in mediaInfoView" :title="mediaInfoItem.title">
            <v-expansion-panel-text>
              <v-list>
                <v-list-item v-for="info in mediaInfoItem.infoList">
                  <v-row>
                    <v-col cols="4" class="font-weight-bold text-grey-darken-1">
                      <v-tooltip v-if="info.tips" :text="info.tips">
                        <template v-slot:activator="{ props }">
                          <p v-bind="props">{{ info.title }}</p>
                        </template>
                      </v-tooltip>
                      <p v-else>{{ info.title }}</p>
                    </v-col>
                    <v-col cols="8" class="text-truncate">
                      <p v-if="typeof info.data === 'string' || typeof info.data === 'number'">
                        {{ info.data.toString() }}
                      </p>
                      <div v-else>
                        <p v-for="[infoKey, infoVal] in Object.entries(info.data)">
                          {{ infoKey }} : {{ infoVal }}
                        </p>
                      </div>
                    </v-col>
                  </v-row>
                </v-list-item>
              </v-list>
            </v-expansion-panel-text>
          </v-expansion-panel>
        </v-expansion-panels>
      </v-card-text>
    </v-card>

    <v-card title="Info raw output">
      <v-card-text>
        <div style="height: 500px">
          <CodeEditor
            v-model:value="rawInfoView"
            language="json"
            theme="vs-dark"
            :options="{ readOnly: true }"
          />
        </div>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<style scoped></style>
