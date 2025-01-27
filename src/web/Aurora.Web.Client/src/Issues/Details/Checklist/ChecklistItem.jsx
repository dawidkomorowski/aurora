import { useState } from "react";
import styles from "./ChecklistItem.module.css"

export function ChecklistItem({ id, content, isChecked, onUpdate, onRemove }) {
    const [editMode, setEditMode] = useState(false);
    const [currentContent, setCurrentContent] = useState(content);

    function handleIsCheckedInput() {
    }

    function handleContentInput(event) {
        setCurrentContent(event.target.value);
    }

    function handleEditButtonClick() {
        setEditMode(true);
    }

    function handleRemoveButtonClick() {
        onRemove(id);
    }

    function handleSaveButtonClick() {
        setEditMode(false);
        onUpdate(id, currentContent, isChecked);
    }

    function handleCancelButtonClick() {
        setEditMode(false);
        setCurrentContent(content);
    }

    let contentElement;
    let buttons;

    if (editMode) {
        contentElement = (
            <>
                <input type="text" value={currentContent} onInput={handleContentInput} style={{ width: "100%", boxSizing: "border-box" }} />
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
        contentElement = (
            <>
                <div style={{ display: "inline-flex" }}>
                    <div>
                        <input type="checkbox" checked={isChecked} onChange={handleIsCheckedInput} />
                    </div>
                    <div>{content}</div>
                </div>
            </>
        );

        buttons = (
            <>
                <button onClick={handleEditButtonClick}>Edit</button>
                <button onClick={handleRemoveButtonClick} style={{ marginLeft: "5px" }}>Remove</button>
            </>
        );
    }

    return (
        <div className={styles.checklistItem} style={{ display: "flex", margin: "5px 0px 5px 0px" }}>
            <div style={{ flexGrow: "1", marginRight: "10px" }}>
                {contentElement}
            </div>
            <div style={{ marginLeft: "auto" }}>
                {buttons}
            </div>
        </div>
    );
}
