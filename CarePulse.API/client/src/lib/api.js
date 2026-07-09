export const apiFetch = async (user, path, options = {}) => {
  let response;

  try {
    response = await fetch(path, {
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${user?.token}`,
      },
      ...options,
    });
  } catch (error) {
    throw new Error(error?.message || 'Network error while calling API.');
  }

  const text = await response.text();
  let payload = null;

  if (text) {
    try {
      payload = JSON.parse(text);
    } catch {
      payload = { message: 'No JSON response.' };
    }
  }

  if (!response.ok) {
    const message = payload?.message || payload?.error || payload?.details || (payload?.errors ? Object.values(payload.errors).flat().join(' ') : response.statusText) || 'Request failed.';
    throw new Error(message);
  }

  return payload;
};
