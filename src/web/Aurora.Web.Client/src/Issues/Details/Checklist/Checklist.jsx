import { useState } from "react";
import { ChecklistItem } from "./ChecklistItem";
import { ChecklistApiClient } from "../../../ApiClients/ChecklistApiClient";

export function Checklist({ checklist, onRemoved }) {
    const [editMode, setEditMode] = useState(false);
    const [title, setTitle] = useState(checklist.title);
    const [previousTitle, setPreviousTitle] = useState(checklist.title);

    function handleTitleInput(event) {
        setTitle(event.target.value);
    }

    function handleEditButtonClick() {
        setEditMode(true);
    }

    function handleSaveButtonClick() {
        ChecklistApiClient.updateChecklist(checklist.id, title).then(responseData => {
            setEditMode(false);
            setTitle(responseData.title);
            setPreviousTitle(responseData.title);
        }).catch(error => {
            console.error(error);
        });

    }

    function handleCancelButtonClick() {
        setEditMode(false);
        setTitle(previousTitle);
    }

    function handleRemoveButtonClick() {
        if (window.confirm("Selected checklist will be removed. Do you want to continue?")) {
            ChecklistApiClient.removeChecklist(checklist.id).then(() => {
                onRemoved();
            }).catch(error => {
                console.error(error);
            });
        }
    }

    let content;
    let buttons;

    if (editMode) {
        content = (
            <>
                <input type="text" value={title} onInput={handleTitleInput} style={{ width: "100%", boxSizing: "border-box" }} />
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
                <strong>{title}</strong>
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

    const items = checklist.items.map(i => <ChecklistItem key={i.id} id={i.id} content={i.content} isChecked={i.isChecked} />);

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