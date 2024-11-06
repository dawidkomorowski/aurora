
export class ApiValidationError extends Error {
    #errorResonse;

    static isValidationError(errorResponse) {
        return errorResponse.type === "https://tools.ietf.org/html/rfc9110#section-15.5.1";
    }

    constructor(errorResponse) {
        super(errorResponse.title);

        this.#errorResonse = errorResponse;
    }

    get title() {
        return this.#errorResonse.title;
    }

    get errors() {
        return this.#errorResonse.errors;
    }

    get errorMessages() {
        return Object.keys(this.#errorResonse.errors).flatMap(key => this.#errorResonse.errors[key]);
    }
}
