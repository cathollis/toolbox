import { PublicClientApplication, type Configuration } from "@azure/msal-browser";

const msalConfig: Configuration = {
  auth: {
    clientId: "9e0a2293-9585-48bc-9a2e-f7acba4dd59a",
    authority: "https://login.microsoftonline.com/common",
    redirectUri: window.location.origin,
  },
};

export function initializeMsal() {
  return msalInstance.initialize();
}

export const msalInstance = new PublicClientApplication(msalConfig);