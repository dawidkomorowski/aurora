import { useState } from "react";

export function NewChecklistItem({ onCreate, onCancel }) {
    const [content, setContent] = useState("");

    function handleContentInput(event) {
        setContent(event.target.value);
    }

    function handleCreateButtonClick() {
        onCreate(content);
    }

    return (
        <div style={{ display: "flex" }}>
            <div style={{ flexGrow: "1", marginRight: "10px" }}>
                <input type="text" value={content} onInput={handleContentInput} style={{ width: "100%", boxSizing: "border-box" }} />
            </div>
            <div style={{ marginLeft: "auto" }}>
                <button onClick={handleCreateButtonClick}>Create</button>
                <button onClick={onCancel} style={{ marginLeft: "5px" }}>Cancel</button>
            </div>
        </div>
    );
}
