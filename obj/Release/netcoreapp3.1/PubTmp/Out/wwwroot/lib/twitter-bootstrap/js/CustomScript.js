function confirmDelete(uniqueId, isDeleteClicked) {// this function is called from ListUsers view to toggle delition buttons
    var deleteSpan = 'deleteSpan_' + uniqueId;
    var confirmDeleteSpan = 'confirmDeleteSpan_' + uniqueId;

    if (isDeleteClicked) {
        $('#' + deleteSpan).hide(); // 'id' selector in jquery starts with #  i.e: $('#test') selects the elements with id="test"
        $('#' + confirmDeleteSpan).show();
    } else {
        $('#' + deleteSpan).show();
        $('#' + confirmDeleteSpan).hide();
    }
}
