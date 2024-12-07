import { ApiValidationError } from "./ApiValidationError";

export class ApiClient {
    static fetch(url, requestInit) {
        return fetch(url, requestInit).then(async (response) => {
            if (!response.ok) {
                if (ApiClient.#hasJsonBody(response)) {
                    const errorResponse = await response.json();

                    if (ApiValidationError.isValidationError(errorResponse)) {
                        throw new ApiValidationError(errorResponse);
                    }
                }

                throw new Error(`Response status: ${response.status}`);
            }

            if (ApiClient.#hasJsonBody(response)) {
                return response.json();
            }

            return Promise.resolve();
        });
    }

    static #hasJsonBody(response) {
        const contentType = response.headers.get("content-type");
        return contentType && contentType.includes("application/json");
    }
}
