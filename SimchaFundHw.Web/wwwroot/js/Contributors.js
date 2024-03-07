$(() => {
    const addPersonModal = new bootstrap.Modal($("#add-person")[0])
    $('#new-person').on('click', function() {
        addPersonModal.show()
    })


    const depositModal = new bootstrap.Modal($("#deposit-modal")[0])
    $('.btn-success').on('click', function () {
        depositModal.show()
    })


})