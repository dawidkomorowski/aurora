import { useState } from "react";
import { ChecklistItem } from "./ChecklistItem";

export function Checklist({ checklist, onRemove }) {
    const [editMode, setEditMode] = useState(false);

    const items = checklist.items.map(i => <ChecklistItem key={i.id} id={i.id} content={i.content} isChecked={i.isChecked} />);

    function handleEditButtonClick() {
        setEditMode(true);
    }

    function handleSaveButtonClick() {
        setEditMode(false);
    }

    function handleCancelButtonClick() {
        setEditMode(false);
    }

    function handleRemoveButtonClick() {
        onRemove(checklist.id);
    }

    let content;
    let buttons;

    if (editMode) {
        content = (
            <>
                <input type="text" style={{ width: "100%", boxSizing: "border-box" }} />
            </>
        );

        buttons = (
            <>
                <button onClick={handleSaveButtonClick}>Save</button>
                <button onClick={handleCancelButtonClick} style={{ marginLeft: "5px" }}>Cancel</button>
            </>
        );
    }
    else {
        content = (
            <>
                <strong>{checklist.title}</strong>
            </>
        );

        buttons = (
            <>
                <button>Add item</button>
                <button onClick={handleEditButtonClick} style={{ marginLeft: "5px" }}>Edit</button>
                <button onClick={handleRemoveButtonClick} style={{ marginLeft: "5px" }}>Remove</button>
            </>
        );
    }

    return (
        <div style={{ borderStyle: "solid", borderWidth: "2px", padding: "10px" }}>
            <div style={{ display: "flex" }}>
                <div style={{ flexGrow: "1", marginRight: "10px" }}>
                    {content}
                </div>
                <div style={{ marginLeft: "auto" }}>
                    {buttons}
                </div>
            </div>
            <div>
                {items}
            </div>
        </div>
    );
}

