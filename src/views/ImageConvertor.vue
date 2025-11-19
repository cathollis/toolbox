<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import type { VNodeRef } from 'vue'
import {
  ColorSpace,
  ImageMagick,
  initializeImageMagick,
  MagickFormat,
  type AsyncImageCallback,
  type IDefine,
  type SyncImageCallback,
} from '@imagemagick/magick-wasm'

import { humanizeBinaryBytes } from '@/utils/humanize.ts'

import wasm from '@imagemagick/magick-wasm/magick.wasm?url'

const webpDesignProfile: IDefine[] = [
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
  {
    format: MagickFormat.WebP,
    name: 'target-size',
    value: '0',
  },
  {
    format: MagickFormat.WebP,
    name: 'preprocessing',
    value: '0',
  },
  {
    format: MagickFormat.WebP,
    name: 'target-psnr',
    value: '42',
  },
  {
    format: MagickFormat.WebP,
    name: 'segments',
    value: '3',
  },
  {
    format: MagickFormat.WebP,
    name: 'filter-sharpness',
    value: '7',
  },
  {
    format: MagickFormat.WebP,
    name: 'filter-strength',
    value: '60',
  },
]

const targetProfileListView = [
  {
    title: 'Design Assets',
    description: 'For UI/UX design images.',
    define: webpDesignProfile,
  },
  {
    title: 'Photograph(Not Impl)',
  },
  {
    title: 'Painting(Not Impl)',
  },
  {
    title: 'B&W Document(Not Impl)',
  },
  {
    title: 'Colour Document(Not Impl)',
  },
]

const pageIsProgressing = ref<boolean>(false)
const isProgressing = ref<boolean>(false)

const diffSizeView = computed<string>(() => {
  const inputSize = inputFile.value?.size
  const targetSize = targetBlob.value?.size
  if (inputSize === null || inputSize === undefined || Number.isNaN(inputSize)) {
    return '-'
  }

  if (targetSize === null || targetSize === undefined || Number.isNaN(targetSize)) {
    return '-'
  }

  const percent = targetSize / inputSize
  const result = Math.round((percent - 1) * 100)
  return `${result}%`
})

const inputFileModel = ref<File | File[] | null>(null)
const inputFile = computed<File | null>(() => {
  if (!inputFileModel.value) return null

  if (Array.isArray(inputFileModel.value)) {
    const fileArray = inputFileModel.value as File[]

    if (fileArray.length <= 0) return null
    if (!fileArray[0]) return null

    return fileArray[0]
  }

  return inputFileModel.value as File
})

const inputFileImageUrlView = computed<string>(() => {
  if (!inputFile.value) return ''

  return URL.createObjectURL(inputFile.value)
})

const inputFileSizeView = computed<string>(() => {
  return humanizeBinaryBytes(inputFile.value?.size)
})

const targetBlob = ref<Blob | null>()
const targetFileImageUrlView = ref<string>('')
const targetFileSizeView = computed<string>(() => {
  return humanizeBinaryBytes(targetBlob.value?.size)
})

const targetProfileIndexModel = ref<number>(0)

const handleFileUpdated = () => {
  if (inputFileImageUrlView.value.length > 0) {
    URL.revokeObjectURL(inputFileImageUrlView.value)
  }

  if (targetFileImageUrlView.value.length > 0) {
    URL.revokeObjectURL(targetFileImageUrlView.value)
  }
}

const getArrayBufferFromUint8Array = (input: Uint8Array): ArrayBuffer => {
  if (input.buffer instanceof ArrayBuffer) {
    // 这里还需要考虑偏移和长度
    return input.buffer.slice(input.byteOffset, input.byteOffset + input.byteLength)
  } else {
    // 是 SharedArrayBuffer，需要复制到新 ArrayBuffer
    return new Uint8Array(input).buffer
  }
}

const targetQualityModel = ref<number>(95)
const handleConvertToWebp = async () => {
  isProgressing.value = true

  if (targetFileImageUrlView.value.length > 0) {
    URL.revokeObjectURL(targetFileImageUrlView.value)
  }

  const targetProfile = targetProfileListView[targetProfileIndexModel.value]

  const outputData = await handleConvertToTarget(
    MagickFormat.WebP,
    targetQualityModel.value,
    targetProfile?.define ?? [],
  )

  if (outputData) {
    const arrayBuffer = getArrayBufferFromUint8Array(outputData)
    targetBlob.value = new Blob([arrayBuffer], { type: 'image/webp' })
    if (targetBlob.value) {
      targetFileImageUrlView.value = URL.createObjectURL(targetBlob.value)
    }
  }

  isProgressing.value = false
}

const handleConvertToTarget = async (
  targetFormat: MagickFormat,
  quality: number,
  defines: IDefine[],
): Promise<Uint8Array | undefined> => {
  if (!inputFile.value) {
    return
  }

  const process: SyncImageCallback<Uint8Array | undefined> = (image) => {
    // read finished
    image.quality = quality

    try {
      defines.forEach((define) => {
        image.settings.setDefine(define.format, define.name, define.value)
      })

      image.colorSpace = ColorSpace.sRGB

      try {
        return image.write<Uint8Array>(targetFormat, (data) => Uint8Array.from(data))
      } catch (writeException) {
        alert(['write failed', writeException])
      }
    } catch (e) {
      alert(['read failed', e])
    }
  }

  const fileData = await inputFile.value.arrayBuffer()
  const result = ImageMagick.read<Uint8Array | undefined>(new Uint8Array(fileData), process)
  return result
}

const handleReset = () => {
  inputFileModel.value = null

  targetBlob.value = null
  targetFileImageUrlView.value = ''
  targetQualityModel.value = 95
}

onMounted(async () => {
  pageIsProgressing.value = true
  fetch(wasm)
    .then((res) => res.arrayBuffer())
    .then(async (assembly) => {
      const validateResult = WebAssembly.validate(assembly)
      if (validateResult) {
        await initializeImageMagick(assembly)
        pageIsProgressing.value = false
      } else {
        alert('Web Assembly valid failed.')
      }
    })
})

const containerRef = ref<VNodeRef | undefined>()
</script>

<template>
  <div :ref="containerRef" style="width: 100%; height: 100%; position: relative">
    <v-container class="d-flex flex-column ga-8">
      <v-overlay
        contained
        class="d-flex align-center justify-center"
        persistent
        v-model="pageIsProgressing"
      >
        <v-progress-circular indeterminate color="primary" />
      </v-overlay>

      <v-card title="Operations">
        <v-card-text>
          <v-file-input
            accept="image/*"
            clearable
            label="Select image file."
            :loading="isProgressing"
            v-model="inputFileModel"
            @change="handleFileUpdated"
          ></v-file-input>

          Quality

          <v-slider
            v-model="targetQualityModel"
            :max="100"
            :min="50"
            :step="5"
            class="align-center"
            hide-details
          >
            <template v-slot:append>
              <v-text-field
                v-model="targetQualityModel"
                density="compact"
                style="width: 80px"
                type="number"
                hide-details
                single-line
              ></v-text-field>
            </template>
          </v-slider>

          <!-- target profile list -->
          <div class="w-auto d-flex flex-nowrap flex-row overflow-x-auto">
            <v-card
              v-for="(profile, index) in targetProfileListView"
              :key="index"
              style="width: 22em; height: 10em"
              class="ma-2 flex-shrink-0"
              :class="index === targetProfileIndexModel ? ' bg-primary' : 'bg-background'"
            >
              <v-card-title>{{ profile.title }}</v-card-title>
              <v-card-text>{{ profile.description }}</v-card-text>
              <v-card-actions class="position-absolute bottom-0">
                <v-btn
                  :disabled="index === targetProfileIndexModel || isProgressing"
                  :color="index === targetProfileIndexModel ? '' : 'primary'"
                  @click="targetProfileIndexModel = index"
                >
                  SELECT PROFILE
                </v-btn>
              </v-card-actions>
            </v-card>
          </div>
        </v-card-text>

        <v-card-actions>
          <v-btn :disabled="!inputFile" :loading="isProgressing" @click="handleConvertToWebp">
            Process
          </v-btn>

          <v-btn :loading="isProgressing" @click="handleReset"> RESET </v-btn>
        </v-card-actions>
      </v-card>

      <v-card title="Result">
        <v-card-text>
          <p>From {{ inputFileSizeView }} to {{ targetFileSizeView }} ({{ diffSizeView }})</p>
          <div class="d-flex flex-row">
            <!-- original -->
            <v-card style="flex-shrink: 0; width: 50%">
              <v-card-title
                >Original<span class="text-body-1"
                  >(blank if not support preview)</span
                ></v-card-title
              >
              <v-card-text>
                <div>detail(10x)(not prefect on mobile):</div>
                <div
                  class="ma-4"
                  :style="{
                    width: '480px',
                    height: '480px',
                    backgroundImage: `url(${inputFileImageUrlView})`,
                    backgroundSize: 10 * 100 + '% auto',
                    backgroundPosition: 'center center',
                    backgroundRepeat: 'no-repeat',
                    border: '2px solid #2196f3',
                    borderRadius: '6px',
                  }"
                ></div>

                <v-img
                  class="ma-4"
                  style="border: 0.2em solid"
                  :src="inputFileImageUrlView"
                ></v-img>
              </v-card-text>
            </v-card>

            <!-- converted -->
            <v-card style="flex-shrink: 0; width: 50%" title="Converted">
              <v-card-text>
                <div>detail(10x)(not prefect on mobile):</div>
                <div
                  class="ma-4"
                  :style="{
                    width: '480px',
                    height: '480px',
                    backgroundImage: `url(${targetFileImageUrlView})`,
                    backgroundSize: 10 * 100 + '% auto',
                    backgroundPosition: 'center center',
                    backgroundRepeat: 'no-repeat',
                    border: '2px solid #2196f3',
                    borderRadius: '6px',
                  }"
                ></div>

                <v-img
                  class="ma-4"
                  style="border: 0.2em solid"
                  :src="targetFileImageUrlView"
                ></v-img>
              </v-card-text>
            </v-card>
          </div>
        </v-card-text>
      </v-card>
    </v-container>
  </div>
</template>

<style scoped></style>
