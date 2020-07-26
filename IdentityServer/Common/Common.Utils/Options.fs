namespace Common.Utils

module Options =
    let noneFails ex =
        function
        | Some r ->
            r
        | None ->
            raise ex
