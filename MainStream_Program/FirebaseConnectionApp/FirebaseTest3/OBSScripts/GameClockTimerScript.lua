obs = obslua
source_name = "GameTimer"  -- Default to "GameTimer" text source

stop_text = "00:00"  -- Default to "00:00" when the timer runs out
mode = "Countdown"  -- Hardcoded mode to "Countdown"
activated = false
timer_active = false
settings_ = nil
orig_time = 0
cur_time = 0
cur_ns = 0
up_when_finished = false
up = false
paused = false
source_text_path = ""  -- Field for the source text path (file picker)

last_read_time = ""  -- Variable to store the last read time from the text file
last_tick_time = 0  -- Variable to store the last time the function was executed (in nanoseconds)

-- Function to update text in the source
function set_time_text(ns, text)
    local ms = math.floor(ns / 1000000)

    if string.match(text, "%%0H") then
        local hours = math.floor(ms / 3600000)
        text = string.gsub(text, "%%0H", string.format("%02d", hours))
    end

    if string.match(text, "%%0M") then
        local minutes = math.floor(ms / 60000) % 60
        text = string.gsub(text, "%%0M", string.format("%02d", minutes))
    end

    if string.match(text, "%%0S") then
        local seconds = math.floor(ms / 1000) % 60
        text = string.gsub(text, "%%0S", string.format("%02d", seconds))
    end

    local source = obs.obs_get_source_by_name(source_name)
    if source ~= nil then
        local settings = obs.obs_data_create()
        obs.obs_data_set_string(settings, "text", text)
        obs.obs_source_update(source, settings)
        obs.obs_data_release(settings)
        obs.obs_source_release(source)
    end
end

-- Function to handle the timer logic
function script_tick()
    -- Check if 500ms have passed since the last tick
    local current_time = obs.os_gettime_ns()
    if current_time - last_tick_time < 500000000 then
        return  -- If less than 500ms, exit the function
    end

    last_tick_time = current_time  -- Update the last tick time

    -- Read the text file for updated time
    local file = io.open(source_text_path, "r")
    if file then
        local file_time = file:read("*all"):gsub("%s+", "")  -- Read the time, strip spaces
        file:close()

        -- Log the raw value read from the file
        -- obs.script_log(obs.LOG_INFO, "Raw time read from file: " .. file_time)

        -- If the file time has changed, update the timer
        if file_time ~= last_read_time then
            last_read_time = file_time
            local minutes, seconds = file_time:match("^(%d+):(%d+)$")

            -- Log minutes and seconds separately
            if minutes and seconds then
                -- obs.script_log(obs.LOG_INFO, "Minutes: " .. minutes)
                -- obs.script_log(obs.LOG_INFO, "Seconds: " .. seconds)

                -- Convert MM:SS to total seconds and update the time displayed
                cur_time = tonumber(minutes) * 60 + tonumber(seconds)
                set_time_text(cur_time * 1000000000, "%0M:%0S")  -- Update the text to show the time read
            else
                -- obs.script_log(obs.LOG_ERROR, "Time format is incorrect in file!")
            end
        end
    else
        -- obs.script_log(obs.LOG_ERROR, "Failed to open the text file: " .. source_text_path)
    end
end

-- Function to start the timer
function start_timer()
    timer_active = true
    orig_time = obs.os_gettime_ns()
end

-- Function to stop the timer
function stop_timer()
    timer_active = false
end

-- Function that runs when the settings are changed
function settings_modified(props, prop, settings)
    return true
end

-- Function to handle the properties (settings) of the plugin
function script_properties()
    local props = obs.obs_properties_create()

    return props
end

-- Function that runs when the plugin is loaded
function script_update(settings)
    stop_timer()
    up = false

    -- The source is now hardcoded to "GameTimer"
    source_name = "GameTimer"

    -- Ensure that the source exists before proceeding
    local source = obs.obs_get_source_by_name(source_name)
    if source then
        -- If source is found, update the settings
    else
        -- obs.script_log(obs.LOG_ERROR, "Source 'GameTimer' not found!")
    end

    -- Dynamically get the script directory where the script is loaded
    local script_dir = debug.getinfo(1, "S").source:match("@?(.*[\\/])")  -- Get the full path of the script
    source_text_path = script_dir .. "CurrentGameInfo\\GameClock.txt"  -- Combine the directory with the relative path

    -- Replace any forward slashes with backslashes for Windows compatibility
    source_text_path = source_text_path:gsub("/", "\\")

    -- Log the full combined path to the OBS log
    obs.script_log(obs.LOG_INFO, "Combined file path: " .. source_text_path)

    cur_time = 0  -- Initialize cur_time

    -- Read the initial time from the file and set the time display
    local file = io.open(source_text_path, "r")
    if file then
        local file_time = file:read("*all"):gsub("%s+", "")  -- Read the time, strip spaces
        file:close()

        -- Log the raw value read from the file
        -- obs.script_log(obs.LOG_INFO, "Raw time read from file: " .. file_time)

        local minutes, seconds = file_time:match("^(%d+):(%d+)$")

        -- If the file time is in valid MM:SS format, set the initial time
        if minutes and seconds then
            cur_time = tonumber(minutes) * 60 + tonumber(seconds)
            set_time_text(cur_time * 1000000000, "%0M:%0S")  -- Set the initial time in the source
        else
            -- obs.script_log(obs.LOG_ERROR, "Invalid time format in the file!")
        end
    else
        -- obs.script_log(obs.LOG_ERROR, "Failed to open the text file: " .. source_text_path)
    end

    -- Start the timer automatically when the plugin is loaded
    start_timer()  
end

-- Function to set default settings
function script_defaults(settings)
    obs.obs_data_set_default_string(settings, "source_text_path", "")  -- Default empty source text path
end

-- Function to describe the plugin
function script_description()
    return "A simple countdown timer for OBS text sources. The timer will always be in Countdown mode and will display in the format minutes:seconds. Time is read from the specified text file."
end

-- Add a timer to execute the script_tick function every 100ms
obs.timer_add(script_tick, 100)
