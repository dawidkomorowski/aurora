import { ApiValidationError } from "./ApiValidationError";

export class ApiClient {
    static fetch(url, requestInit) {
        return fetch(url, requestInit).then(async (response) => {
            if (!response.ok) {
                if (ApiClient.#hasJsonContentType(response)) {
                    const errorResponse = await response.json();

                    if (ApiValidationError.isValidationError(errorResponse)) {
                        throw new ApiValidationError(errorResponse);
                    }
                }

                throw new Error(`Response status: ${response.status}`);
            }

            if (ApiClient.#hasJsonContentType(response)) {
                return response.json();
            }

            return Promise.resolve();
        });
    }

    static #hasJsonContentType(response) {
        const contentType = response.headers.get("content-type");
        return contentType && (contentType.includes("application/json") || contentType.includes("application/problem+json"));
    }
}
