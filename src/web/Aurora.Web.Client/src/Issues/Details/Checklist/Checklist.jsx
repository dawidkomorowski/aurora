import { useState } from "react";
import { ChecklistItem } from "./ChecklistItem";
import { ChecklistApiClient } from "../../../ApiClients/ChecklistApiClient";
import { ApiValidationError } from "../../../ApiClients/ApiValidationError";
import { NewChecklistItem } from "./NewChecklistItem";

export function Checklist({ checklist, onRemoved }) {
    const [editMode, setEditMode] = useState(false);
    const [title, setTitle] = useState(checklist.title);
    const [previousTitle, setPreviousTitle] = useState(checklist.title);
    const [validationError, setValidationError] = useState(null);
    const [isAddingNewItem, setIsAddingNewItem] = useState(false);
    const [items, setItems] = useState(checklist.items);

    function handleTitleInput(event) {
        setTitle(event.target.value);
    }

    function handleAddItemButtonClick() {
        setIsAddingNewItem(true);
    }

    function handleEditButtonClick() {
        setEditMode(true);
        setIsAddingNewItem(false);
    }

    function handleSaveButtonClick() {
        ChecklistApiClient.updateChecklist(checklist.id, title).then(responseData => {
            setEditMode(false);
            setValidationError(null);
            setTitle(responseData.title);
            setPreviousTitle(responseData.title);
            setItems(responseData.items);
        }).catch(error => {
            if (error instanceof ApiValidationError) {
                setValidationError(error.errorMessages[0]);
            }
            else {
                console.error(error);
            }
        });

    }

    function handleCancelButtonClick() {
        setEditMode(false);
        setValidationError(null);
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

    function handleAddItemCreateButtonClick(content) {
        ChecklistApiClient.createChecklistItem(checklist.id, content).then(() => {
            setIsAddingNewItem(false);

            return ChecklistApiClient.get(checklist.id);
        }).then(responseData => {
            setTitle(responseData.title);
            setPreviousTitle(responseData.title);
            setItems(responseData.items);
        }).catch(error => {
            console.error(error);
        });
    }

    function handleAddItemCancelButtonClick() {
        setIsAddingNewItem(false);
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
                <button onClick={handleAddItemButtonClick}>Add item</button>
                <button onClick={handleEditButtonClick} style={{ marginLeft: "5px" }}>Edit</button>
                <button onClick={handleRemoveButtonClick} style={{ marginLeft: "5px" }}>Remove</button>
            </>
        );
    }

    let validationErrorElement = <></>
    if (validationError) {
        validationErrorElement = (
            <div style={{ color: "red" }}>
                {validationError}
            </div>
        );
    }

    const itemElements = items.map(i => <ChecklistItem key={i.id} id={i.id} content={i.content} isChecked={i.isChecked} />);

    let newItemElement = <></>
    if (isAddingNewItem) {
        newItemElement = (
            <div style={{ marginTop: "10px" }}>
                <NewChecklistItem onCreate={handleAddItemCreateButtonClick} onCancel={handleAddItemCancelButtonClick} />
            </div>
        );
    }

    return (
        <div style={{ borderStyle: "solid", borderWidth: "2px", padding: "10px" }}>
            <div style={{ display: "flex" }}>
                <div style={{ flexGrow: "1", marginRight: "10px" }}>
                    {content}
                    {validationErrorElement}
                </div>
                <div style={{ marginLeft: "auto" }}>
                    {buttons}
                </div>
            </div>
            <div>
                {itemElements}
            </div>
            {newItemElement}
        </div>
    );
}