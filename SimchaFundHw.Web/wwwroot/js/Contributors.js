$(() => {
    const addPersonModal = new bootstrap.Modal($("#add-person")[0])
    $('#new-person').on('click', function () {
        addPersonModal.show()
    })


    const depositModal = new bootstrap.Modal($("#deposit-modal")[0])
    $('.btn-success').on('click', function () {
        depositModal.show()
        const row = $(this)
        const name = row.attr('data-contribname')
        const id = row.attr('data-contribid')
        $('#deposit-modal').attr('value', id)

        const title = $('#deposit-modal').find('.modal-title')
        title.append(name)


    })





})