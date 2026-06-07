const STORAGE_KEY = "vr-avatar-heartwatch-demo";

const emptyState = {
  samples: [],
  messages: [],
  events: []
};

export function loadState() {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) {
    return structuredClone(emptyState);
  }

  try {
    return { ...structuredClone(emptyState), ...JSON.parse(raw) };
  } catch {
    return structuredClone(emptyState);
  }
}

export function saveState(state) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
}

export function appendRecord(collection, record) {
  const state = loadState();
  state[collection] = [record, ...state[collection]].slice(0, 200);
  saveState(state);
  return state;
}

export function resetState() {
  localStorage.removeItem(STORAGE_KEY);
  return structuredClone(emptyState);
}

export function exportState() {
  const state = loadState();
  const blob = new Blob([JSON.stringify(state, null, 2)], { type: "application/json" });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = "vr-avatar-heartwatch-session.json";
  anchor.click();
  URL.revokeObjectURL(url);
}

