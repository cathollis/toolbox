const KIBI = 1024
const TEN_KIBI = 10240

export function humanizeBinaryBytes(bytes?: number | undefined): string {
  if (bytes === undefined || isNaN(bytes)) {
    return '-'
  }

  if (bytes <= TEN_KIBI) {
    return `${bytes} B`
  }

  const kibibytes = bytes / KIBI
  if (kibibytes <= TEN_KIBI) {
    return `${kibibytes} KiB`
  }

  const mebibytes = kibibytes / KIBI
  if (mebibytes <= TEN_KIBI) {
    return `${mebibytes} MiB`
  }

  const gibibytes = mebibytes / KIBI
  if (gibibytes <= TEN_KIBI) {
    return `${gibibytes} GiB`
  }

  const tebibytes = gibibytes / KIBI
  return `${tebibytes} TiB`
}
