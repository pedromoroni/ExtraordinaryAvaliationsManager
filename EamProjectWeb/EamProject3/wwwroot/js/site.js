function initializeDataTable() {
    if ($.fn.DataTable.isDataTable("#myTable")) {
        $("#myTable").DataTable().destroy();
    }
    $("#myTable").DataTable();
}



$(document).ready(function () {
    console.log("Script carregado!");

    $(document).on("click", ".teacher-change", function (e) {
        e.preventDefault();
        var url = $(this).attr("href");

        document.getElementById("cancel_manage_request").style.display = "inline-block";
        document.getElementById("save_manage_request").style.display = "inline-block";
        document.getElementById("accept_manage_request").style.display = "none";
        document.getElementById("deny_manage_request").style.display = "none";

        console.log("Carregando professores de:", url);
        console.log("Teste teste tes");

        $.ajax({
            url: url,
            type: "GET",
            success: function (data) {
                $(".panel-teacher").html(data);
            },
            error: function () {
                alert("Error, loading list of Teachers!");
            }
        });

        return false;
    });

    $(document).on("change", ".enter-grades-module", function () {
        console.log("Evento de mudança disparado!"); // Teste

        let selectedOption = $(this).find("option:selected");  // Pega a opção selecionada
        let moduleId = selectedOption.val();  // Módulo ID
        let examDateTime = selectedOption.data("examdatetime");  // Pega o valor de data-examdatetime

        if (!examDateTime || !moduleId) {
            alert("Erro ao processar a seleção.");
            return;
        }

        console.log("Enviando:", { moduleId, examDateTime }); // Debug

        // Atualiza o campo escondido com o examDateTime
        $('#examDateTimeHidden').val(examDateTime);

        // Requisição AJAX para carregar as notas dos estudantes
        $.ajax({
            url: `/Teacher/StudentsGrade`,
            type: "GET",  // Usamos GET para enviar os parâmetros na URL
            data: {
                moduleId: moduleId,
                examDateTime: examDateTime  // Envia diretamente o examDateTime
            },
            success: function (data) {
                $("#students-grades-painel").html(data); // Atualiza a Partial View
            },
            error: function (xhr, status, error) {
                console.error("Erro na requisição:", error); // Debug do erro
            }
        });

        return false;
    });


    // Evento 'change' que funciona mesmo com elementos carregados dinamicamente
    $(document).on("change", ".requestSelect", function () {
        console.log("Evento de mudança disparado!"); // Teste

        let subjectId = $(this).find(":selected").val();

        $.ajax({
            url: `/Student/NewRequestModules?subjectId=${encodeURIComponent(subjectId)}`,
            type: "GET",
            success: function (data) {
                $("#modulesContainer").html(data); // Atualiza a Partial View
            },
        });

        $.ajax({
            url: `/Student/NewRequestTeachers?subjectId=${encodeURIComponent(subjectId)}`,
            type: "GET",
            success: function (data) {
                $("#teachersContainer").html(data); // Atualiza a Partial View
            },
        });

        return false;
    });


    $(document).on("change", ".changeTeacherSelect", function () {
        console.log("Evento de mudança disparado..."); // Teste

        let teacherId = $(this).find(":selected").val();

        $.ajax({
            url: `/Teacher/TeacherInformation?teacherId=${encodeURIComponent(teacherId)}`,
            type: "GET",
            success: function (data) {
                $(".teacher-manage-request").html(data); // Atualiza a Partial View
            },
            error: function () {
                alert("Error, Loading Teachers!");
            }
        });

        return false;
    });


    $(document).on("click", ".load-partial", function (e) {
        e.preventDefault();
        var url = $(this).attr("href");

        console.log("Carregando URL:", url);

        $.ajax({
            url: url,
            type: "GET",
            success: function (data) {
                $(".right").html(data);
                initializeDataTable(); // Atualiza DataTable após carregar a Partial View
            },
            error: function () {
                alert("Error, Loading the Partial View");
            }
        });

        return false;
    });

    initializeDataTable();
});

