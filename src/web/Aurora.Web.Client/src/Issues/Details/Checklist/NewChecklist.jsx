import { useState } from "react";

export function NewChecklist({ onCreate, onCancel }) {
    const [title, setTitle] = useState("");

    function handleTitleInput(event) {
        setTitle(event.target.value);
    }

    function handleCreateButtonClick() {
        onCreate(title);
    }

    return (
        <div style={{ borderStyle: "solid", borderWidth: "2px", padding: "10px" }}>
            <div style={{ display: "flex" }}>
                <div style={{ flexGrow: "1", marginRight: "10px" }}>
                    <input type="text" value={title} onInput={handleTitleInput} style={{ width: "100%", boxSizing: "border-box" }} />
                </div>
                <div style={{ marginLeft: "auto" }}>
                    <button onClick={handleCreateButtonClick}>Create</button>
                    <button onClick={onCancel} style={{ marginLeft: "5px" }}>Cancel</button>
                </div>
            </div>
        </div>
    );
}
