const USER_ID_KEYS = ['userId', 'userID', 'UserId', 'UserID', 'id']

function asObject(value) {
  return value && typeof value === 'object' && !Array.isArray(value) ? value : {}
}

function pickFirstDefinedValue(objects, keys) {
  for (const object of objects) {
    for (const key of keys) {
      const value = object[key]
      if (value !== null && value !== undefined && value !== '') {
        return value
      }
    }
  }

  return null
}

export function normalizeBackendUserPayload(payload) {
  const root = asObject(payload)
  const nestedData = asObject(root.data)
  const nestedUser = asObject(root.user)
  const nestedDataUser = asObject(nestedData.user)

  const candidateObjects = [root, nestedData, nestedUser, nestedDataUser]
  const merged = Object.assign({}, ...candidateObjects)

  const resolvedUserId = pickFirstDefinedValue(candidateObjects, USER_ID_KEYS)
  if (resolvedUserId !== null) {
    merged.userId = resolvedUserId
  }

  return merged
}
