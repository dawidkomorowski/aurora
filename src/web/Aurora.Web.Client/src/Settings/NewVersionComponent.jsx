import { useState, useEffect } from "react";
import { VersionNameValidator } from "./VersionNameValidator";
import { VersionValidationErrorPresenter } from "./VersionValidationErrorPresenter";

export function NewVersionComponent({ onCreate }) {
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
        onCreate(newVersionName);
        setNewVersionName("");
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
