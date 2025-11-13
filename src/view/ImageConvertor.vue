<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
  ColorSpace,
  ImageMagick,
  initializeImageMagick,
  MagickFormat,
  type IDefine,
} from '@imagemagick/magick-wasm'
import wasm from '@imagemagick/magick-wasm/magick.wasm?url'

const isProgressing = ref<boolean>(false)

const handleClear = () => {
  fileModel.value = null
  outputBlob.value = null
  outputImgUrl.value = ''
}

const fileModel = ref<File | File[] | null>(null)
const file = computed<File | null>(() => {
  if (!fileModel.value) return null

  if (Array.isArray(fileModel.value)) {
    const fileArray = fileModel.value as File[]

    if (fileArray.length <= 0) return null
    if (!fileArray[0]) return null

    return fileArray[0]
  }

  return fileModel.value as File
})

const fileImgUrl = computed<string>(() => {
  if (!file.value) return ''

  return URL.createObjectURL(file.value)
})

const outputBlob = ref<Blob | null>()
const outputImgUrl = ref<string>('')

const webpPictureDefines: IDefine[] = [
  {
    format: MagickFormat.WebP,
    name: 'lossless',
    value: 'false',
  },
  {
    format: MagickFormat.WebP,
    name: 'auto-filter',
    value: 'true',
  },
  {
    format: MagickFormat.WebP,
    name: 'image-hint',
    value: 'graph',
  },
  {
    format: MagickFormat.WebP,
    name: 'sharp-yuv',
    value: 'true',
  },
  {
    format: MagickFormat.WebP,
    name: 'method',
    value: '6',
  },
]

const handleConvertToWebp = () => {
  isProgressing.value = true

  if (fileImgUrl.value.length > 0) {
    URL.revokeObjectURL(fileImgUrl.value)
  }

  if (outputImgUrl.value.length > 0) {
    URL.revokeObjectURL(outputImgUrl.value)
  }

  handleConvertToTarget(MagickFormat.WebP, 75, webpPictureDefines)
    .then((data) => {
      if (data) {
        const blobData = new Uint8Array(data.length)
        blobData.set(data)

        outputBlob.value = new Blob([blobData], { type: 'image/webp' })
        if (outputBlob.value) {
          outputImgUrl.value = URL.createObjectURL(outputBlob.value)
        }
      }
    })
    .finally(() => {
      isProgressing.value = false
    })
}

const handleConvertToTarget = async (
  targetFormat: MagickFormat,
  quality: number,
  defines: IDefine[],
): Promise<Uint8Array | undefined> => {
  if (!file.value) return

  const fileData = await file.value.arrayBuffer()

  return new Promise<Uint8Array>((resolver, reject) => {
    ImageMagick.read(new Uint8Array(fileData), (image) => {
      // read finished
      image.quality = quality

      defines.forEach((define) => {
        image.settings.setDefine(define.format, define.name, define.value)
      })

      image.colorSpace = ColorSpace.sRGB
      image.write(targetFormat, (outputData) => {
        resolver(outputData)
      })
    })
  })
}

onMounted(async () => {
  fetch(wasm)
    .then((res) => res.arrayBuffer())
    .then(async function (testbytes) {
      await initializeImageMagick(testbytes)
    })
})
</script>

<template>
  <v-container class="d-flex flex-column ga-8">
    <v-card title="Operations">
      <v-card-text>
        <v-file-input
          accept="image/*"
          clearable
          label="Select image file."
          :loading="isProgressing"
          v-model="fileModel"
        ></v-file-input>
      </v-card-text>
      <v-card-actions>
        <v-btn :disabled="!file" :loading="isProgressing" @click="handleConvertToWebp">
          Process
        </v-btn>

        <v-btn :loading="isProgressing" @click="handleClear"> Clear </v-btn>
      </v-card-actions>
    </v-card>

    <v-card title="Result">
      <v-card-text>
        <p>From {{ file?.size ?? 0 / 1024 }} Kb to {{ outputBlob?.size ?? 0 / 1024 }} Kb</p>
        <p>{{ Math.floor(((outputBlob?.size ?? 0) / (file?.size ?? Infinity)) * 100) }}%</p>
        <div class="d-flex flex-row">
          <!-- original -->
          <v-img :src="fileImgUrl"></v-img>

          <!-- converted -->
          <v-img :src="outputImgUrl"></v-img>
        </div>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<style scoped></style>
