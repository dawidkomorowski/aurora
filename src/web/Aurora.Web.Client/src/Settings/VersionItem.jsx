import { useState } from "react";

export function VersionItem({ id, name }) {
    const [editMode, setEditMode] = useState(false);
    const [versionName, setVersionName] = useState(name);

    function handleEditButtonClick() {
        setEditMode(true);
    }

    function handleSaveButtonClick() {
        setEditMode(false);
    }

    function handleVersionNameInput(event) {
        setVersionName(event.target.value);
    }

    let content;
    if (editMode) {
        content =
            <>
                <input value={versionName} onInput={handleVersionNameInput}></input>
                <div style={{ marginLeft: "5px" }}>
                    <button onClick={handleSaveButtonClick}>Save</button>
                </div>
            </>
    }
    else {
        content =
            <>
                <div>{id} {versionName}</div>
                <div style={{ marginLeft: "5px" }}>
                    <button onClick={handleEditButtonClick}>Edit</button>
                </div>
            </>
    }

    return (
        <div style={{ display: "flex", margin: "5px 5px 5px 0px" }}>
            {content}
        </div>
    );
}
