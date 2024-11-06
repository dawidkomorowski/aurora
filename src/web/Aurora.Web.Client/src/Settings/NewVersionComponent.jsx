import { useState, useEffect } from "react";
import { VersionNameValidator } from "./VersionNameValidator";
import { VersionValidationErrorPresenter } from "./VersionValidationErrorPresenter";
import { VersionApiClient } from "../ApiClients/VersionApiClient";
import { ApiValidationError } from "../ApiClients/ApiValidationError";

export function NewVersionComponent({ onRefreshRequested }) {
    const [newVersionName, setNewVersionName] = useState("");
    const [validationErrors, setValidationErrors] = useState([]);

    useEffect(() => {
        setValidationErrors(VersionNameValidator.validate(newVersionName));
    }, [newVersionName]);

    function handleNewVersionInput(event) {
        const versionName = event.target.value;
        setNewVersionName(versionName);
    }

    function handleCreateButtonClick() {
        VersionApiClient.create(newVersionName).then(() => {
            onRefreshRequested();
            setNewVersionName("");
        }).catch(error => {
            if (error instanceof ApiValidationError) {
                setValidationErrors([...validationErrors, ...error.errorMessages]);
            }
            else {
                console.error(error);
            }
        });
    }

    const hasValidationErrors = validationErrors.length != 0;

    return (
        <div>
            <input type="text" value={newVersionName} onInput={handleNewVersionInput} style={{ marginRight: "5px" }}></input>
            <button onClick={handleCreateButtonClick} disabled={hasValidationErrors}>Create</button>
            <VersionValidationErrorPresenter validationErrors={validationErrors} />
        </div>
    );
}
