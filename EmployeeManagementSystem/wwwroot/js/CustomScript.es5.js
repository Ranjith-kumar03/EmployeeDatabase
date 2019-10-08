'use strict';

function confirmDelete(uniqueId, isDeleteClicked) {
    var deleteSpan = 'deleteSpan_' + uniqueId;
    var confirmDeleteSpan = 'confirmDeleteSpan_' + uniqueId;

    if (isDeleteClicked) {
        $('#' + deleteSpan).hide();
        $('#' + confirmDeleteSpan).show();
    } else {
        $('#' + deleteSpan).show();
        $('#' + confirmDeleteSpan).hide();
    }
}

//function deleteConfirmation(id, delete_or_not) {
//    alert("hello iam working");
//    prompt("What are you doing");
//    var confirmdeleteSpan = 'confirmdeleteSpan_'+id;
//    var deletespan ='deletespan_'+id;
//    if (delete_or_not) {
//        $('#' + deletespan).hide();
//        $('#' + confirmdeleteSpan).show();

//    } else {
//        $('#' + deletespan).show();
//        $('#' + confirmdeleteSpa).hide();
//    }
//}

