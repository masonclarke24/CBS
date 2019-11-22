// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.
function rowClicked(row) {
    if ($(row).css("background-color") !== "rgb(204, 204, 204)" && $(row).find("input[type=radio]").length != 0) {
        $(".selected").removeClass("selected");
        $(row).addClass("selected");

        $(row).find("input[type=radio]").attr("checked", true);
    }
}
function addGolfer(button) {
    var childCount = $("input[name^=Golfers]").length;
    if (childCount > 3) return;
    var formGroup = `<div class="form-group">
                    <label>Member Number</label>
                    <input class="form-control" name="Golfers[` + (childCount).toString() + `]" />
                    <span class="text-danger" \></span>
                </div>`;
    $(button).parent().append(formGroup);
}
function removeGolfer(button) {
    $("input[name^=Golfers]").last().parent().remove()
}
function addErrorMessages(errors) {
    for (var i = 0; i < errors.length; i++) {
        $("input[name=Golfers[" + i.toString() + "]").parent().children("span").text("Member does not exist");
    }
}
function selectAvaliableTime() {
    var teeTimes = $("table").children("tbody").children("tr");
    for (var i = 0; i < teeTimes.length; i++) {
        if ($(teeTimes[i]).attr("style") !== "background-color: #CCC") {
            var radioButton = $(teeTimes[i]).find("input[type=radio]");
            if (radioButton.length == 0) continue;
            radioButton.attr("checked", true);
            $(teeTimes[i]).addClass("selected");
            return;
        }

    }
}
window.document.onload = selectAvaliableTime();